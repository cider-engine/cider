using Cider.Assets;
using Cider.Internals;
using SDL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static SDL.SDL3;

namespace Cider.Render
{
    public class Renderer : IDisposable
    {
        private readonly unsafe SDL_Renderer* _renderer;

        private bool disposedValue;

        private readonly Lazy<ITextEngine> _textEngine;

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

        public Lazy<ITextEngine> TextEngine
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return _textEngine;
            }
        }

        public Camera2D Camera2D { get; set; } = new();

        public ScaleMode ScaleMode
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                unsafe
                {
                    SDL_ScaleMode mode;
                    SDLHelpers.ThrowIfFalse(SDL_GetDefaultTextureScaleMode(_renderer, &mode));
                    return (ScaleMode)mode;
                }
            }

            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                unsafe
                {
                    SDLHelpers.ThrowIfFalse(SDL_SetDefaultTextureScaleMode(_renderer, (SDL_ScaleMode)value));
                }
            }
        }

        public Size LogicalSize
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                SDLHelpers.EnsureOnMainThread();

                unsafe
                {
                    int width, height;
                    SDLHelpers.ThrowIfFalse(SDL_GetRenderLogicalPresentation(_renderer, &width, &height, null));

                    return new(width, height);
                }
            }
        }

        public LogicalPresentationMode LogicalPresentationMode
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                SDLHelpers.EnsureOnMainThread();

                unsafe
                {
                    SDL_RendererLogicalPresentation mode;
                    SDLHelpers.ThrowIfFalse(SDL_GetRenderLogicalPresentation(_renderer, null, null, &mode));

                    return (LogicalPresentationMode)mode;
                }
            }
        }

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

            _textEngine = new(() => new RendererTextEngine(this));
        }

#nullable enable
        public unsafe void SetRenderTarget(Texture? texture)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SDLHelpers.ThrowIfFalse(SDL_SetRenderTarget(_renderer, texture?.Pointer));
        }

        public unsafe void SetLogicalPresentation(Size size, LogicalPresentationMode mode)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SDLHelpers.ThrowIfFalse(SDL_SetRenderLogicalPresentation(_renderer, size.Width, size.Height, (SDL_RendererLogicalPresentation)mode));
        }

        public unsafe void DrawLine(Vector2 point1, Vector2 point2)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            point1 -= Camera2D.OffsetPosition;
            point2 -= Camera2D.OffsetPosition;
            SDL_RenderLine(_renderer, point1.X, point1.Y, point2.X, point2.Y);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (WhiteSinglePixelTexture.IsValueCreated) WhiteSinglePixelTexture.Value.Dispose();
                    if (_textEngine.IsValueCreated) _textEngine.Value.Dispose();
                }

                unsafe
                {
                    SDL_DestroyRenderer(_renderer);
                    GetPointer(this) = null;
                }

                disposedValue = true;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(_renderer))]
            static extern unsafe ref SDL_Renderer* GetPointer(Renderer @this);
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
