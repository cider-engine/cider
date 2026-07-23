using System;
using System.Collections.Generic;

namespace Cider.Assets
{
    public abstract class Asset<T> : IEquatable<Asset<T>> where T : Asset<T>
    {
        private readonly string _path;
        public string Path => OperatingSystem.IsAndroid() ? _path["Assets/".Length..] : _path;
        public string OriginPath => _path;

        public Asset(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(_path = path);
            Lookup.Add(path, GetThis());
        }

        public abstract T GetThis();

#nullable enable
        public static bool operator ==(Asset<T>? a, Asset<T>? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false; // a与b同时为null的情况已在上面处理
            return a.Path == b.Path;
        }

        public static bool operator !=(Asset<T>? a, Asset<T>? b) => !(a == b);

        public bool Equals(Asset<T>? other) => this == other;

        public override bool Equals(object? obj)
        {
            if (obj is Asset<T> asset) return this == asset;
            return false;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public static IDictionary<string, T> Lookup { get; protected set; } = new Dictionary<string, T>();
    }
}
