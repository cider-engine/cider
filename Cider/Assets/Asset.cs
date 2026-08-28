using System;
using System.Collections.Generic;

namespace Cider.Assets
{
    /// <summary>
    /// <para>资源类的基类</para>
    /// <para>在继承此类时需添加<c>SupportedAssetTypesAttribute</c>标记支持的文件后缀名</para>
    /// <para>例如<c>[SupportedAssetTypes(".wav", ".mp3", ".ogg", ".flac")]</c></para>
    /// </summary>
    /// <typeparam name="T">继承此类的类自身</typeparam>
    public abstract class Asset<T> : IEquatable<Asset<T>> where T : Asset<T>
    {
        private readonly string _path;
        /// <summary>
        /// 在加载时使用的路径，可能因操作系统发生变化
        /// </summary>
        public string Path => OperatingSystem.IsAndroid() ? _path["Assets/".Length..] : _path;
        /// <summary>
        /// 传入构造函数的原路径，此路径用于注册进每个类的<c>LookUp</c>字典
        /// </summary>
        public string OriginPath => _path;

        public Asset(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(_path = path);
            Lookup.Add(path, GetThis());
        }

        /// <summary>
        /// 通过此方法获取实际的<typeparamref name="T"/>对象
        /// </summary>
        /// <returns></returns>
        public abstract T GetThis();

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
            return OriginPath.GetHashCode();
        }

        public static IDictionary<string, T> Lookup { get; protected set; } = new Dictionary<string, T>();
    }
}
