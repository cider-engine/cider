using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Assets
{
    public abstract class Asset : IEquatable<Asset>
    {
        private readonly string _path;
        public string Path => OperatingSystem.IsAndroid() ? _path["Assets/".Length..] : _path;

        public Asset(string path) => _path = path;

#nullable enable
        public static bool operator ==(Asset? a, Asset? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false; // a与b同时为null的情况已在上面处理
            return a.Path == b.Path;
        }

        public static bool operator !=(Asset? a, Asset? b) => !(a == b);

        public bool Equals(Asset? other) => this == other;

        public override bool Equals(object? obj)
        {
            if (obj is Asset asset) return this == asset;
            return false;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }
}
