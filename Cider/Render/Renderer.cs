using Cider.Assets;
using Cider.Internals;
using SDL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using static SDL.SDL3;

namespace Cider.Render
{
    public class Renderer : IDisposable
    {
        private readonly unsafe SDL_Renderer* _renderer;

        private bool disposedValue;

        internal readonly Lazy<Texture> WhiteSinglePixelTexture;

        internal unsafe SDL_Renderer* Pointer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return _renderer;
            }
        }

        internal readonly Dictionary<TextureAsset, (CancellationTokenSource source, Task<Texture> task)> Textures = new();

        internal unsafe Renderer(Window window) : this(window.Pointer)
        {}

        internal unsafe Renderer(SDL_Window* window)
        {
            SDLHelpers.EnsureOnMainThread();
            _renderer = SDLHelpers.ThrowIfPtrIsNull(SDL_CreateRenderer(window, (byte*)null));

            WhiteSinglePixelTexture = new(() =>
            {
                using var surface = new Surface(1, 1);
                surface.Clear(Color.White);
                return new(this, surface); // Surface可在创建Texture后直接销毁
            });
        }

        public unsafe void DrawLine(Vector2 point1, Vector2 point2)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SDL_RenderLine(_renderer, point1.X, point1.Y, point2.X, point2.Y);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                unsafe
                {
                    SDL_DestroyRenderer(_renderer);
                }

                disposedValue = true;
            }
        }

        ~Renderer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
