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
                SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions);

        private static readonly SymbolDisplayFormat formatMetadata =
            SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
            .WithGenericsOptions(SymbolDisplayGenericsOptions.None);

        /// <summary>
        /// 返回带global::前缀的全名
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static string GetFullyQualifiedName(this ISymbol symbol) => symbol.ToDisplayString(formatGlobal);

        public static string GetFullMetadataName(this ISymbol symbol) => symbol is INamedTypeSymbol { TypeArguments: { IsEmpty: false, Length: var length } }
            ? $"{symbol.ToDisplayString(formatMetadata)}`{length}"
            : symbol.ToDisplayString(formatMetadata);

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

#nullable enable
        static ISymbol? SearchMemberSymbol(ITypeSymbol? symbol, string name)
        {
            if (symbol is null) return null;
            return symbol.GetMembers(name).SingleOrDefault() ?? SearchMemberSymbol(symbol.BaseType, name);
        }

        // 最好别动
        public static void ProcessElementSetMember(XElement memberOwnerElement,
                        Dictionary<string, Dictionary<string, string>> mappings,
                        IndentedTextWriter writer,
                        List<string>? namedFields,
                        Compilation compilation)
        {
            if (mappings.TryGetValue(memberOwnerElement.Name.NamespaceName, out var dict))
            {
                if (dict.TryGetValue(memberOwnerElement.Name.LocalName, out var memberOwnerMetadataName))
                {
                    foreach (var memberElement in memberOwnerElement.Elements())
                    {
                        var memberOwnerType = compilation.GetTypeByMetadataName(memberOwnerMetadataName);
                        if (memberOwnerType is null)
                        {
                            writer.WriteErrorMessage($"Type {memberOwnerMetadataName} could not be found");
                            return;
                        }

                        var memberSymbol = SearchMemberSymbol(memberOwnerType, memberElement.Name.LocalName);
                        if (memberSymbol is null)
                        {
                            writer.WriteErrorMessage($"Member (property or field or event) {memberElement.Name.LocalName} could not be found in type {memberOwnerType.Name}");
                            return;
                        }

                        if (memberSymbol.Kind == SymbolKind.Event)
                        {
                            writer.Write("Invoke = _this => _this.");
                            writer.Write(memberElement.Name.LocalName);
                            writer.Write(" += ");
                            writer.Write(memberElement.Value);
                            writer.WriteLine(',');
                            continue;
                        }

                        var memberType = (memberSymbol as IPropertySymbol)?.Type ?? (memberSymbol as IFieldSymbol)?.Type;
                        if (memberType is null)
                        {
                            writer.WriteErrorMessage($"Member {memberSymbol.Name} is neither property, field of type {memberOwnerType.Name}");
                            return;
                        }

                        if (memberElement.Value.StartsWith("asset://"))
                        {
                            writer.Write(memberElement.Name.LocalName);
                            writer.Write(" = global::Cider.Assets.AssetManager.");
                            writer.Write(memberElement.Value.Substring("asset://".Length));
                            writer.WriteLine(',');
                        }

                        else switch (memberType.SpecialType)
                            {
                                case SpecialType.System_Boolean:
                                    writer.Write(memberElement.Name.LocalName);
                                    writer.Write(" = ");
                                    writer.Write(memberElement.Value.Trim().ToLowerInvariant());
                                    writer.WriteLine(",");
                                    break;

                                case SpecialType.System_Char:
                                    writer.Write(memberElement.Name.LocalName);
                                    writer.Write(" = '");
                                    writer.Write(memberElement.Value);
                                    writer.WriteLine("',");
                                    break;

                                case > SpecialType.System_Char and < SpecialType.System_String:
                                    writer.Write(memberElement.Name.LocalName);
                                    writer.Write(" = ");
                                    writer.Write(memberElement.Value.Trim());
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
                                    writer.Write(memberElement.Name.LocalName);
                                    if (memberElement.Elements().SingleOrDefault() is XElement { Name: { NamespaceName: CommandNamespaceUri, LocalName: "Static" } } x)
                                    {
                                        writer.Write(" = global::");
                                        writer.Write(x.Value);
                                        writer.WriteLine(',');
                                    }

                                    else
                                    {
                                        writer.Write(" = @\"");
                                        writer.Write(memberElement.Value);
                                        writer.WriteLine("\",");
                                    }
                                    break;



                                default:
                                    switch (memberType.TypeKind)
                                    {
                                        case TypeKind.Enum:
                                            writer.Write(memberElement.Name.LocalName);
                                            if (memberElement.Elements().ToArray() is XElement[] { Length: > 1 } array)
                                            {
                                                writer.Write(" = ");
                                                writer.Write(memberType.GetFullyQualifiedName());
                                                writer.Write('.');
                                                writer.Write(array[0].Value);
                                                foreach (var item in array.AsSpan(1))
                                                {
                                                    writer.Write(" | ");
                                                    writer.Write(memberType.GetFullyQualifiedName());
                                                    writer.Write('.');
                                                    writer.Write(item.Value);
                                                }
                                            }

                                            else
                                            {
                                                writer.Write(" = ");
                                                writer.Write(memberType.GetFullyQualifiedName());
                                                writer.Write('.');
                                                writer.Write(memberElement.Value);
                                            }
                                            writer.WriteLine(',');
                                            break;

                                        case TypeKind.Array:
                                            writer.Write(memberElement.Name.LocalName);
                                            writer.WriteLine(" = ([");
                                            writer.Indent++;
                                            ProcessElementCreateObject(memberElement, mappings, writer, namedFields, compilation);
                                            writer.Indent--;
                                            writer.WriteLine("]),");
                                            break;

                                        default:
                                            if (memberType is INamedTypeSymbol
                                                {
                                                    ConstructedFrom.SpecialType:
                                                        SpecialType.System_Collections_Generic_IList_T or SpecialType.System_Collections_Generic_ICollection_T
                                                }
                                            || memberType.Interfaces.Any(static x =>
                                                x.ConstructedFrom.SpecialType is
                                                    SpecialType.System_Collections_Generic_IList_T or SpecialType.System_Collections_Generic_ICollection_T))
                                            {
                                                writer.Write(memberElement.Name.LocalName);
                                                writer.WriteLine(" = {([");
                                                writer.Indent++;
                                                ProcessElementCreateObject(memberElement, mappings, writer, namedFields, compilation);
                                                writer.Indent--;
                                                writer.WriteLine("])},");
                                                break;
                                            }

                                            else
                                            {
                                                writer.Write(memberElement.Name.LocalName);
                                                writer.Write(" = ");
                                                if (memberElement.HasElements)
                                                    ProcessElementCreateObject(memberElement, mappings, writer, namedFields, compilation);

                                                else
                                                {
                                                    if (SearchMemberSymbol(memberType, memberElement.Value) is IPropertySymbol { IsStatic: true } staticMember)
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
                                                        writer.Write(memberElement.Value);
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

            else if (memberOwnerElement.Name.Namespace == CommandNamespace)
            {
                writer.WriteErrorMessage($"Error command namespace of element: {memberOwnerElement.Name.LocalName}");
                return;
            }

            else
            {
                writer.WriteErrorMessage($"Unknown namespace: {memberOwnerElement.Name.NamespaceName}");
                return;
            }
        }

        public static void ProcessElementCreateObject(XElement outerPropertyElement,
                        Dictionary<string, Dictionary<string, string>> mappings,
                        IndentedTextWriter writer,
                        List<string>? namedFields,
                        Compilation compilation)
        {
            foreach (var objectElement in outerPropertyElement.Elements())
            {
                if (mappings.TryGetValue(objectElement.Name.NamespaceName, out var dict))
                {
                    if (!dict.TryGetValue(objectElement.Name.LocalName, out string? objectTypeMetadataName))
                    {
                        writer.WriteErrorMessage($"The type: {objectElement.Name.LocalName} is not registered in the namespace: {objectElement.Name.NamespaceName}");
                        return;
                    }

                    INamedTypeSymbol? objectType = compilation.GetTypeByMetadataName(objectTypeMetadataName);

                    if (objectType is null)
                    {
                        writer.WriteErrorMessage($"Cannot find the type: {objectTypeMetadataName}");
                        return;
                    }

                    var objectTypeFullName = objectType.GetFullyQualifiedName();

                    if (!objectType.TypeArguments.IsEmpty) // 不支持直接实例化有泛型参数的类
                    {
                        writer.WriteErrorMessage($"Generic is not supported for the type: {objectTypeFullName}");
                        return;
                    }

                    if (objectElement.Attribute(CommandWithClass) is XAttribute attr1) // <Component x:Class="A" /> -> (Component)new global::A()
                    {
                        if (objectElement.Attribute(CommandWithName) is XAttribute attr2)
                        {
                            writer.WriteLine($"(this.{attr2.Value} = new global::{attr1.Value}()");
                            namedFields?.Add($"internal global::{attr1.Value} {attr2.Value};");
                        }

                        else writer.WriteLine($"(({objectTypeFullName})new global::{attr1.Value}()");
                    }

                    else if (objectElement.Attribute(CommandWithName) is XAttribute attr2)
                    {
                        writer.WriteLine($"(this.{attr2.Value} = new {objectTypeFullName}()");
                        namedFields?.Add($"internal {objectTypeFullName} {attr2.Value};");
                    }

                    else writer.WriteLine($"(new {objectTypeFullName}()");

                    writer.WriteLine('{');
                    writer.Indent++;

                    foreach (var objectMember in objectElement.Attributes())
                    {
                        if (objectMember.Name.Namespace == CommandNamespace) continue;
                        var value = objectMember.Value;
                        if (value.Length > 0 && value[0] == '@')
                        {
                            writer.Write(objectMember.Name.LocalName);
                            writer.Write(" = ");
                            writer.Write(value.Substring(1));
                            writer.WriteLine(',');
                        }

                        else if (value.StartsWith("asset://"))
                        {
                            writer.Write(objectMember.Name.LocalName);
                            writer.Write(" = global::Cider.Assets.AssetManager.");
                            writer.Write(value.Substring("asset://".Length));
                            writer.WriteLine(',');
                        }

                        else if (objectType is not null)
                        {
                            var objectMemberSymbol = SearchMemberSymbol(objectType, objectMember.Name.LocalName);

                            if (objectMemberSymbol is { Kind: SymbolKind.Event })
                            {
                                writer.Write("Invoke = _this => _this.");
                                writer.Write(objectMember.Name.LocalName);
                                writer.Write(" += ");
                                writer.Write(value);
                                writer.WriteLine(',');
                                continue;
                            }

                            var objectMemberType = (objectMemberSymbol as IPropertySymbol)?.Type ?? (objectMemberSymbol as IFieldSymbol)?.Type;

                            if (objectMemberType is not null)
                            {
                                writer.Write(objectMember.Name.LocalName);
                                writer.Write(" = ");

                                switch (objectMemberType.SpecialType)
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

                                        switch (objectMemberType.SpecialType)
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
                                        if (objectMemberType.TypeKind == TypeKind.Enum)
                                        {
                                            writer.Write(objectMemberType.GetFullyQualifiedName());
                                            writer.Write('.');
                                            writer.Write(value.Trim());
                                            writer.WriteLine(',');
                                        }

                                        else
                                        {
                                            writer.Write(objectMemberType.GetFullyQualifiedName());
                                            writer.Write(".Parse(\"");
                                            writer.Write(value);
                                            writer.WriteLine("\"),");
                                        }
                                        break;
                                }
                            }

                            else
                            {
                                writer.WriteErrorMessage($"Cannot find the type of member: {objectMemberSymbol}; current containingType: {objectType}");
                                return;
                            }
                        }
                    }

                    if (objectElement.HasElements) ProcessElementSetMember(objectElement, mappings, writer, namedFields, compilation);
                    // 不是为null就return了吗咋这里还报nullable
                    else if (!string.IsNullOrWhiteSpace(objectElement.Value) && objectType!.GetAttributes().SingleOrDefault(static x => x.AttributeClass?.GetFullyQualifiedName() == "global::Cider.Attributes.ContentAttribute") is AttributeData attribute)
                    {
                        if (attribute.ConstructorArguments[0].Value is string contentProperty)
                        {
                            var contentPropertySymbol = SearchMemberSymbol(objectType, contentProperty);

                            var contentPropertyType = (contentPropertySymbol as IPropertySymbol)?.Type ?? (contentPropertySymbol as IFieldSymbol)?.Type;

                            if (contentPropertyType is not null)
                            {
                                writer.Write($"{contentProperty} = ");
                                switch (contentPropertyType.SpecialType)
                                {
                                    case SpecialType.System_Enum:
                                        writer.Write(contentPropertyType.GetFullyQualifiedName());
                                        writer.Write('.');
                                        writer.Write(objectElement.Value.Trim());
                                        writer.WriteLine(',');
                                        break;

                                    case SpecialType.System_Boolean:
                                        writer.Write(objectElement.Value.Trim().ToLowerInvariant());
                                        writer.WriteLine(',');
                                        break;

                                    case SpecialType.System_Char:
                                        writer.Write('\'');
                                        writer.Write(objectElement.Value);
                                        writer.WriteLine("',");
                                        break;

                                    case > SpecialType.System_Char and < SpecialType.System_String:
                                        writer.Write(objectElement.Value.Trim());

                                        switch (contentPropertyType.SpecialType)
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
                                        writer.Write(objectElement.Value);
                                        writer.WriteLine("\",");
                                        break;

                                    default:
                                        if (contentPropertyType.TypeKind == TypeKind.Enum)
                                        {
                                            writer.Write(contentPropertyType.GetFullyQualifiedName());
                                            writer.Write('.');
                                            writer.Write(objectElement.Value.Trim());
                                            writer.WriteLine(',');
                                        }

                                        else
                                        {
                                            writer.Write(contentPropertyType.GetFullyQualifiedName());
                                            writer.Write(".Parse(\"");
                                            writer.Write(objectElement.Value);
                                            writer.WriteLine("\"),");
                                        }
                                        break;
                                }
                            }

                            else
                            {
                                writer.WriteErrorMessage($"Cannot find the type of member: {contentPropertySymbol}; current containingType: {objectType}");
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

                // 直接使用元素内容，忽略所有子元素
                // 默认情况下只有在处理集合元素时才会尝试用Command命名空间的元素直接创建对象
                else if (objectElement.Name.Namespace == CommandNamespace)
                {
                    switch (objectElement.Name.LocalName)
                    {
                        // <Component x:Class="MyComponent" />约等于<x:Class>MyComponent</x:Class>
                        // 后者不会强转类型为Component
                        case "Class":
                            writer.Write("new global::");
                            writer.Write(objectElement.Value);
                            writer.WriteLine("(),");
                            break;

                        case "Number":
                            writer.Write(objectElement.Value);
                            writer.WriteLine(',');
                            break;

                        case "String":
                            writer.Write('"');
                            writer.Write(objectElement.Value);
                            writer.WriteLine("\",");
                            break;

                        case "Asset":
                            writer.Write("global::Cider.Assets.AssetManager.");
                            writer.Write(objectElement.Value.Substring("asset://".Length));
                            writer.WriteLine(',');
                            break;

                        case "Enum":
                            writer.Write("global::");
                            if (objectElement.Attribute("Type") is XAttribute attr)
                            {
                                writer.Write(attr.Value);
                                writer.Write('.');
                            }
                            writer.Write(objectElement.Value);
                            writer.WriteLine(',');
                            break;

                        default:
                            writer.WriteErrorMessage($"Current element: {outerPropertyElement}, child: {objectElement}");
                            return;
                    }
                }

                else
                {
                    writer.WriteErrorMessage($"Unknown namespace: {objectElement.Name.NamespaceName}");
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
