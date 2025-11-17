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
    public partial class CiderXmlGenerator
    {
        public static void SceneInitialize(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<AlwaysEqualWrapper<Dictionary<string, Dictionary<string, string>>>> xmlnsContext)
        {
            var provider = context.AdditionalTextsProvider
                .Where(static x => x.Path.EndsWith(".scene.cider"))
                .Combine(xmlnsContext)
                .Combine(context.CompilationProvider)
                .Select(static (x, token) =>
                {
                    var ((additionalText, mappingsWrapper), compilation) = x;
                    var mappings = mappingsWrapper.Value;

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

                    var @class = root.Attribute(CommandWithClass)?.Value;

                    string fullName = null;
                    if (mappings.TryGetValue(root.Name.NamespaceName, out var dict))
                        if (dict.TryGetValue(root.Name.LocalName, out fullName))

                    if (fullName is null || @class is null) return default;

                    using var stringWriter = new StringWriter();
                    using var writer = new IndentedTextWriter(stringWriter, "    ");

                    var separatorIndex = @class.LastIndexOf('.');

                    if (separatorIndex > -1)
                    {
                        var sceneNamespace = @class.Substring(0, separatorIndex);
                        writer.WriteLine($"namespace {sceneNamespace};");
                        writer.WriteLine();
                    }

                    var sceneClass = @class.Substring(separatorIndex + 1); // 不用特殊处理，-1 + 1 = 0
                    writer.WriteLine($$"""
                        public partial class {{sceneClass}} : global::{{fullName}}
                        {
                            public {{sceneClass}}()
                            {
                                Children.AddRange([
                        """);

                    writer.Indent = 3;

                    var namedFields = new List<string>();

                    if (root.Element(DefaultWithChildren) is XElement children)
                        ProcessElement(children, mappings, writer, namedFields, compilation, false);

                    writer.Indent = 0;

                    writer.WriteLine("""
                                    ]);
                            }
                        """);

                    writer.WriteLine();

                    foreach (var field in namedFields)
                    {
                        writer.Indent = 1;
                        writer.WriteLine(field);
                    }

                    writer.Indent = 0;

                    writer.WriteLine('}');

                    writer.Flush();

                    return (stringWriter.ToString(), @class);
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
