using Microsoft.CodeAnalysis;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public const string DefaultNamespaceUri = "https://github.com/cider-engine";
        public const string CommandNamespaceUri = "https://github.com/cider-engine/command";

        public static readonly XNamespace DefaultNamespace = DefaultNamespaceUri;
        public static readonly XNamespace CommandNamespace = CommandNamespaceUri;

        public static readonly XName DefaultWithChildren = DefaultNamespace + "Children";

        public static readonly XName CommandWithClass = CommandNamespace + "Class";
        public static readonly XName CommandWithName = CommandNamespace + "Name";
        public static readonly XName CommandWithCollection = CommandNamespace + "Collection";
        public static readonly XName CommandWithValue = CommandNamespace + "Value";

        public static void WriteErrorMessage(this IndentedTextWriter writer, string message = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "")
        {
            if (writer is null) return;

            var msg = (message ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ').Trim();
            if (string.IsNullOrWhiteSpace(msg))
            {
                msg = "An error occurred in Cider Generator.";
            }

            writer.WriteLine($"#error Cider Generator Error: {msg}");

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
            static ISymbol? SearchMember(ITypeSymbol? symbol, string name)
            {
                if (symbol is null) return null;
                return symbol.GetMembers(name).SingleOrDefault() ?? SearchMember(symbol.BaseType, name);
            }

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
                                writer.WriteErrorMessage($"Type {fullName} could not be found");
                                return;
                            }

                            var member = SearchMember(type, child.Name.LocalName);
                            if (member is null)
                            {
                                writer.WriteErrorMessage($"Member (property or field or event) {child.Name.LocalName} could not be found in type {type.Name}");
                                return;
                            }

                            if (member.Kind == SymbolKind.Event)
                            {
                                writer.Write("Invoke = _this => _this.");
                                writer.Write(child.Name.LocalName);
                                writer.Write(" += ");
                                writer.Write(child.Value);
                                writer.WriteLine(',');
                                continue;
                            }

                            var memberType = (member as IPropertySymbol)?.Type ?? (member as IFieldSymbol)?.Type;
                            if (memberType is null)
                            {
                                writer.WriteErrorMessage($"Member {member.Name} is neither property, field of type {type.Name}");
                                return;
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
                                    case SpecialType.System_Boolean:
                                        writer.Write(child.Name.LocalName);
                                        writer.Write(" = ");
                                        writer.Write(child.Value.Trim().ToLowerInvariant());
                                        writer.WriteLine(",");
                                        break;

                                    case SpecialType.System_Char:
                                        writer.Write(child.Name.LocalName);
                                        writer.Write(" = '");
                                        writer.Write(child.Value);
                                        writer.WriteLine("',");
                                        break;

                                    case > SpecialType.System_Char and < SpecialType.System_String:
                                        writer.Write(child.Name.LocalName);
                                        writer.Write(" = ");
                                        writer.Write(child.Value.Trim());
                                        switch (memberType.SpecialType)
                                        {
                                            case SpecialType.System_Int64:
                                                writer.Write('L');
                                                break;

                                            case SpecialType.System_UInt64:
                                                writer.Write("uL");
                                                break;

                                            case SpecialType.System_Decimal:
                                                writer.Write('m');
                                                break;

                                            case SpecialType.System_Single:
                                                writer.Write('f');
                                                break;

                                            case SpecialType.System_Double:
                                                writer.Write('d');
                                                break;
                                        }
                                        writer.WriteLine(',');
                                        break;

                                    case SpecialType.System_String:
                                        writer.Write(child.Name.LocalName);
                                        if (child.Elements().SingleOrDefault() is XElement { Name: { NamespaceName: CommandNamespaceUri, LocalName: "Static" } } x)
                                        {
                                            writer.Write(" = global::");
                                            writer.Write(x.Value);
                                            writer.WriteLine(',');
                                        }

                                        else
                                        {
                                            writer.Write(" = @\"");
                                            writer.Write(child.Value);
                                            writer.WriteLine("\",");
                                        }
                                        break;



                                    default:
                                        switch (memberType.TypeKind)
                                        {
                                            case TypeKind.Enum:
                                                writer.Write(child.Name.LocalName);
                                                if (child.Elements().ToArray() is XElement[] { Length: > 1 } array)
                                                {
                                                    writer.Write(" = global::");
                                                    writer.Write(memberType.GetFullMetadataName());
                                                    writer.Write('.');
                                                    writer.Write(array[0].Value);
                                                    foreach (var item in array.AsSpan(1))
                                                    {
                                                        writer.Write(" | global::");
                                                        writer.Write(memberType.GetFullMetadataName());
                                                        writer.Write('.');
                                                        writer.Write(item.Value);
                                                    }
                                                }

                                                else
                                                {
                                                    writer.Write(" = global::");
                                                    writer.Write(memberType.GetFullMetadataName());
                                                    writer.Write('.');
                                                    writer.Write(child.Value);
                                                }
                                                writer.WriteLine(',');
                                                break;

                                            case TypeKind.Array:
                                                writer.Write(child.Name.LocalName);
                                                writer.WriteLine(" = ([");
                                                writer.Indent++;
                                                ProcessElement(child, mappings, writer, namedFields, compilation, false);
                                                writer.Indent--;
                                                writer.WriteLine("]),");
                                                break;

                                            default:
                                                if (memberType.Interfaces.Any(static x => x.SpecialType is > SpecialType.System_Array and <= SpecialType.System_Collections_Generic_IReadOnlyCollection_T))
                                                {
                                                    writer.Write(child.Name.LocalName);
                                                    writer.WriteLine(" = {([");
                                                    writer.Indent++;
                                                    ProcessElement(child, mappings, writer, namedFields, compilation, false);
                                                    writer.Indent--;
                                                    writer.WriteLine("])},");
                                                    break;
                                                }

                                                else
                                                {
                                                    writer.Write(child.Name.LocalName);
                                                    writer.Write(" = ");
                                                    if (child.HasElements)
                                                        ProcessElement(child, mappings, writer, namedFields, compilation, false);

                                                    else
                                                    {
                                                        if (SearchMember(memberType, child.Value) is IPropertySymbol { IsStatic: true } staticMember)
                                                        {
                                                            writer.Write(memberType.GetFullyQualifiedName());
                                                            writer.Write('.');
                                                            writer.Write(staticMember.Name);
                                                            writer.WriteLine(',');
                                                        }

                                                        else
                                                        {
                                                            writer.Write(memberType.GetFullyQualifiedName());
                                                            writer.Write(".Parse(\"");
                                                            writer.Write(child.Value);
                                                            writer.WriteLine("\"),");
                                                        }
                                                    }
                                                    break;
                                                }
                                        }

                                        break;
                                }
                        }
                    }
                }

                else if (element.Name.Namespace == CommandNamespace)
                {
                    writer.WriteErrorMessage($"Error command namespace of element: {element.Name.LocalName}");
                    return;
                }

                else
                {
                    writer.WriteErrorMessage($"Unknown namespace: {element.Name.NamespaceName}");
                    return;
                }
            }

            else
                foreach (var child in element.Elements())
                {
                    if (mappings.TryGetValue(child.Name.NamespaceName, out var dict))
                    {
                        if (!dict.TryGetValue(child.Name.LocalName, out string? fullName))
                        {
                            writer.WriteErrorMessage($"The type: {child.Name.LocalName} is not registered in the namespace: {child.Name.NamespaceName}");
                            return;
                        }

                        if (child.Attribute(CommandWithClass) is XAttribute attr1)
                        {
                            if (child.Attribute(CommandWithName) is XAttribute attr2)
                            {
                                writer.WriteLine($"(this.{attr2.Value} = new global::{attr1.Value}()");
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

                        writer.WriteLine('{');
                        writer.Indent++;

                        INamedTypeSymbol? containingType = compilation.GetTypeByMetadataName(fullName);

                        if (containingType is null)
                        {
                            writer.WriteErrorMessage($"Cannot find the type: {fullName}");
                            return;
                        }

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

                            else if (containingType is not null)
                            {
                                var member = SearchMember(containingType, attr.Name.LocalName);

                                if (member is { Kind: SymbolKind.Event })
                                {
                                    writer.Write("Invoke = _this => _this.");
                                    writer.Write(attr.Name.LocalName);
                                    writer.Write(" += ");
                                    writer.Write(value);
                                    writer.WriteLine(',');
                                    continue;
                                }

                                var memberType = (member as IPropertySymbol)?.Type ?? (member as IFieldSymbol)?.Type;

                                if (memberType is not null)
                                {
                                    writer.Write(attr.Name.LocalName);
                                    writer.Write(" = ");

                                    switch (memberType.SpecialType)
                                    {
                                        case SpecialType.System_Boolean:
                                            writer.Write(value.Trim().ToLowerInvariant());
                                            writer.WriteLine(',');
                                            break;

                                        case SpecialType.System_Char:
                                            writer.Write('\'');
                                            writer.Write(value);
                                            writer.WriteLine("',");
                                            break;

                                        case > SpecialType.System_Char and < SpecialType.System_String:
                                            writer.Write(value.Trim());

                                            switch (memberType.SpecialType)
                                            {
                                                case SpecialType.System_Int64:
                                                    writer.Write('L');
                                                    break;

                                                case SpecialType.System_UInt64:
                                                    writer.Write("uL");
                                                    break;

                                                case SpecialType.System_Decimal:
                                                    writer.Write('m');
                                                    break;

                                                case SpecialType.System_Single:
                                                    writer.Write('f');
                                                    break;

                                                case SpecialType.System_Double:
                                                    writer.Write('d');
                                                    break;
                                            }

                                            writer.WriteLine(',');

                                            break;

                                        case SpecialType.System_String:
                                            writer.Write("@\"");
                                            writer.Write(value);
                                            writer.WriteLine("\",");
                                            break;

                                        default:
                                            if (memberType.TypeKind == TypeKind.Enum)
                                            {
                                                writer.Write(memberType.GetFullyQualifiedName());
                                                writer.Write('.');
                                                writer.Write(value.Trim());
                                                writer.WriteLine(',');
                                            }

                                            else
                                            {
                                                writer.Write(memberType.GetFullyQualifiedName());
                                                writer.Write(".Parse(\"");
                                                writer.Write(value);
                                                writer.WriteLine("\"),");
                                            }
                                            break;
                                    }
                                }

                                else
                                {
                                    writer.WriteErrorMessage($"Cannot find the type of member: {member}; current containingType: {containingType}");
                                    return;
                                }
                            }
                        }

                        if (child.HasElements) ProcessElement(child, mappings, writer, namedFields, compilation, true);
                        // 不是为null就return了吗咋这里还报nullable
                        else if (!string.IsNullOrWhiteSpace(child.Value) && containingType!.GetAttributes().SingleOrDefault(static x => x.AttributeClass?.GetFullyQualifiedName() == "global::Cider.Attributes.ContentAttribute") is AttributeData attribute)
                        {
                            if (attribute.ConstructorArguments[0].Value is string contentProperty)
                            {
                                var member = SearchMember(containingType, contentProperty);

                                var memberType = (member as IPropertySymbol)?.Type ?? (member as IFieldSymbol)?.Type;

                                if (memberType is not null)
                                {
                                    writer.Write($"{contentProperty} = ");
                                    switch (memberType.SpecialType)
                                    {
                                        case SpecialType.System_Enum:
                                            writer.Write(memberType.GetFullyQualifiedName());
                                            writer.Write('.');
                                            writer.Write(child.Value.Trim());
                                            writer.WriteLine(',');
                                            break;

                                        case SpecialType.System_Boolean:
                                            writer.Write(child.Value.Trim().ToLowerInvariant());
                                            writer.WriteLine(',');
                                            break;

                                        case SpecialType.System_Char:
                                            writer.Write('\'');
                                            writer.Write(child.Value);
                                            writer.WriteLine("',");
                                            break;

                                        case > SpecialType.System_Char and < SpecialType.System_String:
                                            writer.Write(child.Value.Trim());

                                            switch (memberType.SpecialType)
                                            {
                                                case SpecialType.System_Int64:
                                                    writer.Write('L');
                                                    break;

                                                case SpecialType.System_UInt64:
                                                    writer.Write("uL");
                                                    break;

                                                case SpecialType.System_Decimal:
                                                    writer.Write('m');
                                                    break;

                                                case SpecialType.System_Single:
                                                    writer.Write('f');
                                                    break;

                                                case SpecialType.System_Double:
                                                    writer.Write('d');
                                                    break;
                                            }

                                            writer.WriteLine(',');

                                            break;

                                        case SpecialType.System_String:
                                            writer.Write("@\"");
                                            writer.Write(child.Value);
                                            writer.WriteLine("\",");
                                            break;

                                        default:
                                            if (memberType.TypeKind == TypeKind.Enum)
                                            {
                                                writer.Write(memberType.GetFullyQualifiedName());
                                                writer.Write('.');
                                                writer.Write(child.Value.Trim());
                                                writer.WriteLine(',');
                                            }

                                            else
                                            {
                                                writer.Write(memberType.GetFullyQualifiedName());
                                                writer.Write(".Parse(\"");
                                                writer.Write(child.Value);
                                                writer.WriteLine("\"),");
                                            }
                                            break;
                                    }
                                }

                                else
                                {
                                    writer.WriteErrorMessage($"Cannot find the type of member: {member}; current containingType: {containingType}");
                                    return;
                                }
                            }

                            else
                            {
                                writer.WriteErrorMessage($"The ContentAttribute {attribute.AttributeClass} has no correct argument");
                            }
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

                            case "Enum":
                                writer.Write("global::");
                                if (child.Attribute("Type") is XAttribute attr)
                                {
                                    writer.Write(attr.Value);
                                    writer.Write('.');
                                }
                                writer.Write(child.Value);
                                writer.WriteLine(',');
                                break;

                            default:
                                writer.WriteErrorMessage($"Current element: {element}, child: {child}");
                                return;
                        }
                    }

                    else
                    {
                        writer.WriteErrorMessage($"Unknown namespace: {child.Name.NamespaceName}");
                        return;
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
