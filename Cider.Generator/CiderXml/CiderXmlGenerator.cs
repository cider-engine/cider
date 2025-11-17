using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Cider.Generator.CiderXml
{
    [Generator]
    public partial class CiderXmlGenerator : IIncrementalGenerator
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

                        #region 直接引入类
                        foreach (var (xmlns, type) in assembly.GetAttributes()
                            .Where(static attr => attr.AttributeClass.Name == "XmlnsDirectTypeAttribute")
                            .Select(static attr => (xmlns: (string)attr.ConstructorArguments[0].Value,
                                type: (INamedTypeSymbol)attr.ConstructorArguments[1].Value)))
                        {
                            if (!mappings.TryGetValue(xmlns, out var dict))
                            {
                                mappings.Add(xmlns, dict = new());
                            }
                            dict[type.Name] = type.GetFullMetadataName();
                        }
                        #endregion

                        if (namespaces.Count == 0) continue;

                        //foreach (var ns in assembly.GlobalNamespace.GetNamespaceMembers()) ProcessNamespace(ns, namespaces);
                        ProcessNamespace(assembly.GlobalNamespace, namespaces); // 允许不在命名空间中声明类

                        static void ProcessNamespace(INamespaceSymbol @namespace, Dictionary<string, Dictionary<string, string>> namespaces)
                        {
                            var name = @namespace.ToDisplayString();
                            if (namespaces.ContainsKey(name))
                                foreach (var type in @namespace.GetTypeMembers())
                                {
                                    namespaces[name].Add(type.Name, type.GetFullMetadataName());
                                }

                            foreach (var ns in @namespace.GetNamespaceMembers()) ProcessNamespace(ns, namespaces);
                        }
                    }
                    return new AlwaysEqualWrapper<Dictionary<string, Dictionary<string, string>>>(mappings);
                });

            ProjectSettingsInitialize(context, xmlnsContext);
            SceneInitialize(context, xmlnsContext);
        }
    }
}
