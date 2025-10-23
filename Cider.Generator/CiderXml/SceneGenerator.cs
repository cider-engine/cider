using Microsoft.CodeAnalysis;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static Cider.Generator.GeneratorHelper;

namespace Cider.Generator.CiderXml
{
    [Generator]
    public class SceneGenerator : IIncrementalGenerator
    {

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var xmlnsContext = context.CompilationProvider
                .Select(static (x, token) =>
                {
                    var mappings = new Dictionary<string /* xmlns命名空间 */, Dictionary<string /* 类名 */, string /* 类的完整限定名 */>>();
                    foreach (var assembly in x.SourceModule.ReferencedAssemblySymbols)
                    {
                        var namespaces = new Dictionary<string /* 命名空间 */, Dictionary<string, string> /* 同上面那个嵌套的 */>();
                        foreach (var (xmlns, ns) in assembly.GetAttributes()
                            .Where(static attr => attr.AttributeClass.Name == "XmlnsDefinitionAttribute")
                            .Select(static attr => (xmlns: (string)attr.ConstructorArguments[0].Value, ns: (string)attr.ConstructorArguments[1].Value)))
                        {
                            if (mappings.TryGetValue(xmlns, out var value))
                                namespaces.Add(ns, value);

                            else
                            {
                                mappings.Add(xmlns, value = new()); // 保证两个Dictionary的Value指向同一个Dictionary<string, string>
                                namespaces.Add(ns, value);
                            }
                        }

                        if (namespaces.Count == 0) continue;

                        foreach (var ns in assembly.GlobalNamespace.GetNamespaceMembers()) ProcessNamespace(ns, namespaces);

                        static void ProcessNamespace(INamespaceSymbol @namespace, Dictionary<string, Dictionary<string, string>> namespaces)
                        {
                            var name = @namespace.ToDisplayString();
                            if (namespaces.ContainsKey(name))
                                foreach (var type in @namespace.GetTypeMembers())
                                {
                                    namespaces[name].Add(type.Name, type.GetFullyQualifiedName());
                                }

                            foreach (var ns in @namespace.GetNamespaceMembers()) ProcessNamespace(ns, namespaces);
                        }
                    }
                    return mappings;
                });

            var provider = context.AdditionalTextsProvider
                .Where(static x => x.Path.EndsWith(".scene.cider.xml"))
                .Combine(xmlnsContext)
                .Select(static (x, token) =>
                {
                    var (additionalText, mappings) = x;

                    var text = additionalText.GetText(token)?.ToString();
                    if (text is null) return default;

                    XElement root;
                    try
                    {
                        root = XElement.Parse(text);
                    }
                    catch
                    {
                        return default;
                    }

                    var @class = root.Attribute(CommandNamespace + "Class")?.Value;

                    if (root.Name != DefaultNamespace + "Scene" || @class is null) return default;

                    using var stringWriter = new StringWriter();
                    using var writer = new IndentedTextWriter(stringWriter, "    ");

                    var separatorIndex = @class.LastIndexOf('.');

                    if (separatorIndex < 1) return default;

                    var sceneNamespace = @class.Substring(0, separatorIndex);
                    var sceneClass = @class.Substring(separatorIndex + 1);
                    writer.WriteLine($$"""
                        namespace {{sceneNamespace}}
                        {
                            public partial class {{sceneClass}} : global::Cider.Components.Scene
                            {
                                protected override void OnLoaded(global::Cider.Components.Scene scene)
                                {
                                    Children.AddRange([
                        """);

                    writer.Indent = 4;

                    ProcessElement(root, mappings, writer);

                    writer.Indent = 0;

                    writer.WriteLine($$"""
                                    ]);

                                    base.OnLoaded(scene);
                                }
                            }
                        }
                        """);

                    writer.Flush();

                    return (stringWriter.ToString(), @class);

                    static void ProcessElement(XElement element, Dictionary<string, Dictionary<string, string>> mappings, IndentedTextWriter writer)
                    {
                        foreach (var child in element.Elements().Where(static x => !IsAttribute(x.Parent, x)))
                        {
                            if (mappings.TryGetValue(element.Name.NamespaceName, out var dict))
                            {
                                if (dict.TryGetValue(child.Name.LocalName, out var fullName))
                                    writer.WriteLine($"new {fullName}()");

                                writer.WriteLine('{');
                                writer.Indent++;
                                foreach (var attr in child.Attributes())
                                {
                                    var value = attr.Value;
                                    if (value.Length > 0 && value[0] == '@')
                                    {
                                        writer.Write(attr.Name.LocalName);
                                        writer.Write(" = ");
                                        writer.Write(value.Substring(1));
                                        writer.WriteLine(',');
                                    }

                                    else
                                    {
                                        writer.Write(attr.Name.LocalName);
                                        writer.Write(" = new global::Cider.Converters.StringValueConverter(\"");
                                        writer.Write(value);
                                        writer.WriteLine("\"),");
                                    }
                                }
                                if (child.HasElements)
                                {
                                    foreach (var attr in child.Elements().Where(static x => IsAttribute(x.Parent, x)))
                                    {
                                        var count = attr.Elements().Count();

                                        if (count == 1)
                                        {
                                            var name = attr.Name.LocalName;
                                            writer.Write(name.Substring(name.IndexOf('.') + 1));
                                            writer.Write(" = ");
                                            ProcessElement(attr, mappings, writer);
                                        }

                                        else if (count > 1)
                                        {

                                            var name = attr.Name.LocalName;
                                            writer.Write(name.Substring(name.IndexOf('.') + 1));
                                            writer.WriteLine(" = {");
                                            ProcessElement(attr, mappings, writer);
                                            writer.WriteLine("},");
                                        }
                                    }

                                    writer.WriteLine("Children = {");
                                    writer.Indent++;
                                    ProcessElement(child, mappings, writer);
                                    writer.Indent--;
                                    writer.WriteLine('}');
                                }
                                writer.Indent--;
                                writer.WriteLine("},");
                            }
                        }
                    }

                    static bool IsAttribute(XElement element, XElement child)
                    {
                        if (element.Name.Namespace != child.Name.Namespace) return false;

                        var parentName = element.Name.LocalName.AsSpan();
                        var childName = child.Name.LocalName.AsSpan();
                        var dotPos = childName.IndexOf('.');
                        if (dotPos < 0) return false;

                        return parentName.SequenceEqual(childName.Slice(0, dotPos));
                    }
                });

            context.RegisterSourceOutput(provider, static (context, x) =>
            {
                var (content, name) = x;

                if (content is null || name is null) return;

                context.AddSource($"{name}.Scene.g.cs", content);
            });
        }
    }
}
