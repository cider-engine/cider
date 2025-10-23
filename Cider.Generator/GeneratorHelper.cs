using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Cider.Generator
{
    public static class GeneratorHelper
    {
        private static readonly SymbolDisplayFormat format =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        /// <summary>
        /// 返回带global::前缀的全名
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static string GetFullyQualifiedName(this ISymbol symbol) => symbol.ToDisplayString(format);


        public static readonly XNamespace DefaultNamespace = "https://github.com/cider-engine";
        public static readonly XNamespace CommandNamespace = "https://github.com/cider-engine/command";
    }
}
