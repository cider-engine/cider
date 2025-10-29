using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class SupportedAssetTypesAttribute : Attribute
    {
        public SupportedAssetTypesAttribute(params string[] assetTypes)
        {
            AssetTypes = assetTypes;
        }
        public string[] AssetTypes { get; }
    }
}
