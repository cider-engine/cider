using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Assets
{
    public abstract class Asset : IDisposable
    {
        private bool _disposed;

        public string Path { get; }

        public Asset(string path) => Path = path;

        public abstract bool IsLoaded { get; }

        public abstract void Load();

        public abstract void Unload();

        public abstract object Get(); // 在子类重写时返回具体类型

        public virtual void Dispose()
        {
            if (_disposed) return;
            GC.SuppressFinalize(this);
            if (IsLoaded)
                Unload();
            _disposed = true;
        }

        ~Asset()
        {
            if (IsLoaded)
                Unload();
        }

#nullable enable
        protected Action<Asset>? _onLoaded;
        protected Action<Asset>? _onUnloaded;
#nullable disable

        public event Action<Asset> OnLoaded
        {
            add => _onLoaded += value;
            remove => _onLoaded -= value;
        }
        public event Action<Asset> OnUnloaded
        {
            add => _onUnloaded += value;
            remove => _onUnloaded -= value;
        }
#nullable enable
        public static bool operator ==(Asset? a, Asset? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false; // a与b同时为null的情况已在上面处理
            return a.Path == b.Path;
        }

        public static bool operator !=(Asset? a, Asset? b) => !(a == b);
    }
}
