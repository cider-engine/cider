using Microsoft.CodeAnalysis;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Cider.Generator
{
    public static class GeneratorHelper
    {
        private static readonly SymbolDisplayFormat formatGlobal =
            SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        private static readonly SymbolDisplayFormat formatMetadata =
            SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

        /// <summary>
        /// 返回带global::前缀的全名
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static string GetFullyQualifiedName(this ISymbol symbol) => symbol.ToDisplayString(formatGlobal);

        public static string GetFullMetadataName(this ISymbol symbol) => symbol.ToDisplayString(formatMetadata);


        public static readonly XNamespace DefaultNamespace = "https://github.com/cider-engine";
        public static readonly XNamespace CommandNamespace = "https://github.com/cider-engine/command";

        public static readonly XName DefaultWithChildren = DefaultNamespace + "Children";

        public static readonly XName CommandWithClass = CommandNamespace + "Class";
        public static readonly XName CommandWithName = CommandNamespace + "Name";
        public static readonly XName CommandWithCollection = CommandNamespace + "Collection";
        public static readonly XName CommandWithValue = CommandNamespace + "Value";

        /// 计划（伪代码）：
        /// - 验证 writer 非空
        /// - 规范化 message：替换换行、修 trim、限制长度，使用默认提示（如果为空）
        /// - 写入一行 #error，内容为单行的错误摘要（编译器显示）
        /// - 写入若干注释行，包含文件路径、行号、调用成员、时间戳，便于定位
        /// - 泛型重载复用同样逻辑并返回 default(T)
        public static void WriteErrorMessage(this IndentedTextWriter writer, string message = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "")
        {
            if (writer == null) return;

            // 规范化 message，移除换行并限制长度，避免破坏 #error 指令格式
            var msg = (message ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
            if (string.IsNullOrWhiteSpace(msg))
            {
                msg = "An error occurred in Cider Generator.";
            }

            const int maxLength = 240;
            if (msg.Length > maxLength)
            {
                msg = msg.Substring(0, maxLength).TrimEnd() + "...";
            }

            // 写入编译器可见的错误（单行）
            writer.WriteLine($"#error Cider Generator Error: {msg}");

            // 写入定位信息作为注释，便于在生成的代码中查看详细上下文
            writer.WriteLine($"// File: {callerFilePath}");
            writer.WriteLine($"// Member: {callerMemberName}");
            writer.WriteLine($"// Line: {callerLineNumber}");
        }

        public static T WriteErrorMessage<T>(this IndentedTextWriter writer, string message = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "")
        {
            WriteErrorMessage(writer, message, callerMemberName, callerLineNumber, callerFilePath);
            return default;
        }

        // 最好别动
#nullable enable
        public static void ProcessElement(XElement element,
                        Dictionary<string, Dictionary<string, string>> mappings,
                        IndentedTextWriter writer,
                        List<string>? namedFields,
                        Compilation compilation,
                        bool isSettingProperty)
        {
            if (isSettingProperty)
            {
                if (mappings.TryGetValue(element.Name.NamespaceName, out var dict))
                {
                    if (dict.TryGetValue(element.Name.LocalName, out var fullName))
                    {
                        foreach (var child in element.Elements())
                        {
                            var type = compilation.GetTypeByMetadataName(fullName);
                            if (type is null)
                            {
                                writer.WriteErrorMessage();
                                return;
                            }

                            var member = SearchMember(type, child.Name.LocalName);
                            if (member is null)
                            {
                                writer.WriteErrorMessage();
                                return;
                            }

                            var memberType = (member as IPropertySymbol)?.Type ?? (member as IFieldSymbol)?.Type;
                            if (memberType is null)
                            {
                                writer.WriteErrorMessage();
                                return;
                            }

                            static ISymbol? SearchMember(INamedTypeSymbol? symbol, string name)
                            {
                                if (symbol is null) return null;
                                return symbol.GetMembers(name).SingleOrDefault() ?? SearchMember(symbol.BaseType, name);
                            }

                            if (child.Value.StartsWith("asset://"))
                            {
                                writer.Write(child.Name.LocalName);
                                writer.Write(" = global::Cider.Assets.AssetManager.");
                                writer.Write(child.Value.Substring("asset://".Length));
                                writer.WriteLine(',');
                            }

                            else switch (memberType.SpecialType)
                                {
                                    case > SpecialType.System_Char and < SpecialType.System_String:
                                        writer.Write(child.Name.LocalName);
                                        writer.Write(" = ");
                                        writer.Write(child.Value);
                                        writer.WriteLine(",");
                                        break;

                                    case SpecialType.System_String:
                                        writer.Write(child.Name.LocalName);
                                        writer.Write(" = \"");
                                        writer.Write(child.Value);
                                        writer.WriteLine("\",");
                                        break;

                                    case SpecialType.System_Char:
                                        writer.Write(child.Name.LocalName);
                                        writer.Write(" = '");
                                        writer.Write(child.Value);
                                        writer.WriteLine("',");
                                        break;

                                    case SpecialType.System_Enum:
                                        writer.Write(child.Name.LocalName);
                                        writer.Write(" = global::");
                                        writer.Write(memberType.MetadataName);
                                        writer.Write('.');
                                        writer.Write(child.Value);
                                        writer.WriteLine(',');
                                        break;

                                    case SpecialType.System_Array:
                                        writer.Write(child.Name.LocalName);
                                        writer.WriteLine(" = [");
                                        writer.Indent++;
                                        ProcessElement(child, mappings, writer, namedFields, compilation, false);
                                        writer.Indent--;
                                        writer.WriteLine("],");
                                        break;

                                    default:
                                        if (memberType.Interfaces.Any(static x => x.SpecialType is > SpecialType.System_Array and <= SpecialType.System_Collections_Generic_IReadOnlyCollection_T))
                                        {
                                            writer.Write(child.Name.LocalName);
                                            if (member is IPropertySymbol
                                                {
                                                    SetMethod: null
                                                } or IFieldSymbol
                                                {
                                                    IsReadOnly: true
                                                })
                                                writer.WriteLine(" = {");
                                            else
                                            {
                                                writer.WriteLine(" = new()");
                                                writer.WriteLine('{');
                                            }
                                            writer.Indent++;
                                            ProcessElement(child, mappings, writer, namedFields, compilation, false);
                                            writer.Indent--;
                                            writer.WriteLine("},");
                                            break;
                                        }

                                        else
                                        {
                                            writer.Write(child.Name.LocalName);
                                            writer.Write(" = ");
                                            ProcessElement(child, mappings, writer, namedFields, compilation, false);
                                            break;
                                        }
                                }
                        }
                    }
                }
            }

            else foreach (var child in element.Elements())
                {
                    if (mappings.TryGetValue(child.Name.NamespaceName, out var dict))
                    {
                        if (dict.TryGetValue(child.Name.LocalName, out var fullName))
                        {
                            if (child.Attribute(CommandWithClass) is XAttribute attr1)
                            {
                                if (child.Attribute(CommandWithName) is XAttribute attr2)
                                {
                                    writer.WriteLine($"(this.{attr2.Value} = (global::{fullName})new global::{attr1.Value}()");
                                    namedFields?.Add($"internal global::{attr1.Value} {attr2.Value};");
                                }

                                else writer.WriteLine($"(({fullName})new global::{attr1.Value}()");
                            }

                            else if (child.Attribute(CommandWithName) is XAttribute attr2)
                            {
                                writer.WriteLine($"(this.{attr2.Value} = new global::{fullName}()");
                                namedFields?.Add($"internal global::{fullName} {attr2.Value};");
                            }

                            else writer.WriteLine($"(new global::{fullName}()");
                        }

                        else writer.WriteLine("(new()");

                        writer.WriteLine('{');
                        writer.Indent++;

                        foreach (var attr in child.Attributes())
                        {
                            if (attr.Name.Namespace == CommandNamespace) continue;
                            var value = attr.Value;
                            if (value.Length > 0 && value[0] == '@')
                            {
                                writer.Write(attr.Name.LocalName);
                                writer.Write(" = ");
                                writer.Write(value.Substring(1));
                                writer.WriteLine(',');
                            }

                            else if (value.StartsWith("asset://"))
                            {
                                writer.Write(attr.Name.LocalName);
                                writer.Write(" = global::Cider.Assets.AssetManager.");
                                writer.Write(value.Substring("asset://".Length));
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

                        if (child.HasElements) ProcessElement(child, mappings, writer, namedFields, compilation, true);

                        else if (!string.IsNullOrWhiteSpace(child.Value))
                        {
                            writer.Write("Content = new global::Cider.Converters.StringValueConverter(\"");
                            writer.Write(child.Value);
                            writer.WriteLine("\"),");
                        }

                        writer.Indent--;
                        writer.WriteLine("}),");
                    }

                    else if (child.Name.Namespace == CommandNamespace) // 处理集合元素
                    {
                        switch (child.Name.LocalName)
                        {
                            case "Class":
                                writer.Write("new global::");
                                writer.Write(child.Value);
                                writer.WriteLine("(),");
                                break;

                            case "Number":
                                writer.Write(child.Value);
                                writer.WriteLine(',');
                                break;

                            case "String":
                                writer.Write('"');
                                writer.Write(child.Value);
                                writer.WriteLine("\",");
                                break;

                            case "Asset":
                                writer.Write("global::Cider.Assets.AssetManager.");
                                writer.Write(child.Value.Substring("asset://".Length));
                                writer.WriteLine(',');
                                break;

                            default:
                                writer.WriteErrorMessage();
                                return;
                        }
                    }
                }
        }
    }

    public class AlwaysEqualWrapper<T> : IEquatable<AlwaysEqualWrapper<T>>
    {
        public T Value { get; }
        public AlwaysEqualWrapper(T value)
        {
            Value = value;
        }
        public bool Equals(AlwaysEqualWrapper<T> other)
        {
            return true;
        }
        public override bool Equals(object obj)
        {
            return obj is AlwaysEqualWrapper<T>;
        }
        public override int GetHashCode()
        {
            return 0;
        }
    }
}
