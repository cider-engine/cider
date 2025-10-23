using Microsoft.CodeAnalysis;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static Cider.Generator.GeneratorHelper;

namespace Cider.Generator.CiderXml
{
    [Generator]
    public class ProjectSettingsGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var settings = context.AdditionalTextsProvider
                .Where(static x => x.Path.EndsWith("projectSettings.cider.xml"))
                .Collect()
                .Select(static (x, token) =>
                {
                    var file = x.OrderBy(static x => x.Path.Length).FirstOrDefault();
                    if (file is null) return null;
                    var text = file.GetText(token);
                    if (text is null) return null;

                    XElement root;
                    try
                    {
                        root = XElement.Parse(text.ToString());
                    }
                    catch
                    {
                        return null;
                    }

                    using var stringWriter = new StringWriter();
                    using var writer = new IndentedTextWriter(stringWriter, "    ");

                    writer.Indent = 0;

                    writer.WriteLine("""
                        using var game = new global::Cider.CiderGame(new()
                        {
                        """);

                    writer.Indent = 1;

                    ProcessElements(root, writer);

                    writer.Indent = 0;

                    writer.Write("""
                        });
                        game.Run();
                        """);

                    return stringWriter.ToString();

                    static void ProcessElements(XElement root, IndentedTextWriter writer)
                    {
                        foreach (var attr in root.Attributes())
                        {
                            if (attr.IsNamespaceDeclaration) continue;
                            writer.Write(attr.Name.LocalName);
                            writer.Write(" = ");
                            if (attr.Name.Namespace == CommandNamespace)
                            {
                                switch (attr.Name.LocalName)
                                {
                                    case "MainScene":
                                        writer.Write("new global::");
                                        writer.Write(attr.Value);
                                        writer.WriteLine("(),");
                                        break;
                                }
                            }

                            else
                            {
                                writer.Write("new global::Cider.Converters.StringValueConverter(\"");
                                writer.Write(attr.Value);
                                writer.WriteLine("\"),");
                            }
                        }

                        if (root.HasElements)
                            foreach (var element in root.Elements())
                            {
                                writer.WriteLine("new()");
                                writer.WriteLine('{');
                                writer.Indent++;
                                ProcessElements(element, writer);
                                writer.Indent--;
                                writer.WriteLine("},");
                            }

                        else
                        {
                            if (!string.IsNullOrWhiteSpace(root.Value))
                            {
                                writer.Write(root.Name.LocalName);
                                writer.Write(" = ");
                                writer.Write("new global::Cider.Converters.StringValueConverter(\"");
                                writer.Write(root.Value);
                                writer.WriteLine("\"),");
                            }
                        }
                    }
                });

            context.RegisterSourceOutput(settings, static (context, settings) =>
            {
                if (settings is null) return;
                context.AddSource("CiderGame.ProjectSettings.g.cs", settings);
            });
        }
    }
}
