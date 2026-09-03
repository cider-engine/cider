using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal class SupportedAssetTypesAttribute : Attribute
    {
        public SupportedAssetTypesAttribute(params string[] assetTypes)
        {
            AssetTypes = assetTypes;
        }
        public string[] AssetTypes { get; }
    }
}
