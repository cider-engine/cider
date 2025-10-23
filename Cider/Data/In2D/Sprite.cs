using Microsoft.Xna.Framework.Graphics;
using System;

namespace Cider.Data.In2D
{
    public class Sprite : IResourceOwner, IDisposable
    {
#nullable enable
        private bool _disposed;

        private MonoGame.Extended.Graphics.Sprite? underlyingSprite;

        public string? Source
        {
            get => _disposed ? throw new ObjectDisposedException(nameof(Sprite)) : field;
            set
            {
                ObjectDisposedException.ThrowIf(_disposed, this);

                if (UnloadResourceWhenUnreachable && underlyingSprite is not null)
                {
                    underlyingSprite = null;
                }

                if (value is not null) underlyingSprite = new(Texture2D.FromFile(CiderGame.Instance.GraphicsDevice, field = value));
                else field = value;
            }
        }

        public bool UnloadResourceWhenUnreachable { get; set; } = true;

        public static implicit operator MonoGame.Extended.Graphics.Sprite?(Sprite sprite) => sprite._disposed ? throw new ObjectDisposedException(nameof(Sprite)) : sprite.underlyingSprite;

        public void Dispose()
        {
            if (_disposed) return;
            GC.SuppressFinalize(this);

            Source = null;

            _disposed = true;
        }

        ~Sprite()
        {
            // 能调用这里肯定没有Dispose，直接走普通流程
            Source = null;
        }
    }
}
