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
        public static void ProjectSettingsInitialize(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<AlwaysEqualWrapper<Dictionary<string, Dictionary<string, string>>>> xmlnsContext)
        {
            var projectPath = context.AnalyzerConfigOptionsProvider
                .Select(static (x, token) =>
            {
                if (x.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out var projectPath))
                {
                    return projectPath;
                }
                return null;
            });

            var settings = context.AdditionalTextsProvider
                .Where(static x => x.Path.EndsWith("projectSettings.cider"))
                .Combine(projectPath)
                .Combine(xmlnsContext)
                .Combine(context.CompilationProvider)
                .Select(static (x, token) =>
                {
                    var (((additionalText, projectPath), mappingsWrapper), compilation) = x;
                    if (additionalText.Path != Path.Combine(projectPath, "projectSettings.cider")) return null;
                    var text = additionalText.GetText(token);
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
                        namespace Cider;

                        public static class GameHelper
                        {
                            public static global::Cider.CiderGame NewGame() => new global::Cider.CiderGame(new()
                            {
                        """);

                    writer.Indent = 2;

                    ProcessElement(root, mappingsWrapper.Value, writer, null, compilation, true);

                    writer.Indent = 0;

                    writer.Write("""
                            });
                        }
                        """);

                    writer.Flush();

                    return stringWriter.ToString();
                });

            context.RegisterSourceOutput(settings, static (context, settings) =>
            {
                if (settings is null) return;
                context.AddSource("CiderGame.ProjectSettings.g.cs", settings);
            });
        }
    }
}
