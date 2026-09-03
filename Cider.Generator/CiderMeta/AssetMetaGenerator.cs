using Microsoft.CodeAnalysis;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Cider.Generator.CiderMeta
{
    [Generator]
    public class AssetMetaGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = context.AnalyzerConfigOptionsProvider.Select(static (x, token) =>
            {
                if (x.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out var projectPath))
                {
                    return projectPath;
                }
                return null;
            });

            var registeredAssetTypesProvider = context.CompilationProvider
                .Select(static (x, token) =>
                {
                    var cider = x.SourceModule.ReferencedAssemblySymbols.Single(static y => y.Name == "Cider"); // 只取Cider程序集里的
                    
                    var types = new List<(string, List<string>)>(); // 资源类名，支持的资源类型数组

                    ProcessNamespace(cider.GlobalNamespace, types);

                    return new AlwaysEqualWrapper<List<(string, List<string>)>>(types);

                    static void ProcessNamespace(INamespaceSymbol @namespace, List<(string, List<string>)> types)
                    {
                        foreach (var type in @namespace.GetTypeMembers())
                        {
                            if (type.GetAttributes().SingleOrDefault(static attr => attr.AttributeClass.GetFullyQualifiedName() == "global::Cider.Attributes.SupportedAssetTypesAttribute") is AttributeData attr)
                            {
                                types.Add((type.GetFullyQualifiedName(), attr.ConstructorArguments[0].Values.Select(static x => (string)x.Value).ToList()));
                            }
                        }

                        foreach (var memberNamespace in @namespace.GetNamespaceMembers())
                        {
                            ProcessNamespace(memberNamespace, types);
                        }
                    }
                });

            var provider = context.AdditionalTextsProvider.Where(static x => x.Path.EndsWith(".meta"))
                .Collect()
                .Combine(registeredAssetTypesProvider)
                .Combine(projectPathProvider)
                .Select((x, token) =>
                {
                    var ((additionalTexts, typesWrapper), projectPath) = x;

                    var types = typesWrapper.Value;

                    if (string.IsNullOrEmpty(projectPath))
                    {
                        return null;
                    }

                    using var stringWriter = new StringWriter();
                    using var writer = new IndentedTextWriter(stringWriter, "    ");

                    // 手动控制缩进
                    //writer.Indent = 0;

                    writer.WriteLine("""
                        namespace Cider.Assets;

                        public static class AssetManager
                        {
                            // 触发类的初始化
                            public static void TriggerInit()
                            {}
                        """);

                    //writer.Indent = 1;

                    foreach (var additionalText in additionalTexts)
                    {
                        var text = additionalText.GetText(token)?.ToString();
                        if (text is null) continue;

                        // standard2.0不支持Path.GetRelativePath凑合用吧

                        var fullPath = Path.ChangeExtension(additionalText.Path, null);
                        var relativePath = fullPath.Substring(projectPath.Length + 1).Replace('\\', '/'); // MSBuildProjectDirectory不包含最终反斜杠，不考虑项目在根目录的情况

                        var extension = Path.GetExtension(fullPath);
                        if (string.IsNullOrEmpty(extension)) continue;

#nullable enable
                        var (assetClass, _) = types.Find(x => x.Item2.Contains(extension));
                        if (assetClass is null) continue;
                        JsonDocument? doc = null;
#nullable disable
                        try
                        {
                            doc = JsonDocument.Parse(text);
                            var uid = doc.RootElement.GetProperty("uid").GetString();

                            writer.WriteLine($$"""
                                    /// <summary>
                                    /// 存储路径：<c>{{relativePath}}</c>
                                    /// </summary>
                                    public static readonly {{assetClass}} {{uid}} = new("{{relativePath}}");
                                """);
                        }
                        catch
                        {
                            continue;
                        }
                        finally
                        {
                            doc?.Dispose();
                        }
                    }

                    //writer.Indent = 0;
                    writer.WriteLine('}');

                    return stringWriter.ToString();
                });

            context.RegisterSourceOutput(provider, (context, collectedAssets) =>
            {
                if (collectedAssets is null) return;
                context.AddSource("AssetManager.AssetMeta.g.cs", collectedAssets);
            });
        }
    }
}
