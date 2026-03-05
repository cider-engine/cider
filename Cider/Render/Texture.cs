using Cider.Extensions;
using Cider.Internals;
using SDL;
using System;
using System.Collections.Generic;

namespace Cider.Render
{
    public class Texture : IDisposable
    {
        private bool disposedValue;
        private readonly unsafe SDL_Texture* _texture;
        private Renderer ownerRenderer;

        public Renderer OwnerRenderer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return ownerRenderer;
            }
        }

        public unsafe Texture(Renderer renderer, int width, int height, TextureAccess access)
        {
            ownerRenderer = renderer;

            _texture = SDLHelpers.ThrowIfPtrIsNull(SDL3.SDL_CreateTexture(renderer.Pointer,
                SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888,
                (SDL_TextureAccess)access, width, height));
        }

        public unsafe Texture(Renderer renderer, Surface surface)
        {
            ownerRenderer = renderer;

            _texture = SDLHelpers.ThrowIfPtrIsNull(SDL3.SDL_CreateTextureFromSurface(renderer.Pointer, surface.Pointer));
        }

        public unsafe Texture(Renderer renderer, string path)
        {
            ownerRenderer = renderer;

            using var unmanaged = path.ToUnmanagedUtf8();
            _texture = SDLHelpers.ThrowIfPtrIsNull(SDL3_image.IMG_LoadTexture(renderer.Pointer, unmanaged.Pointer));
        }

        internal unsafe Texture(Renderer renderer, SDL_IOStream* stream)
        {
            ownerRenderer = renderer;

            _texture = SDLHelpers.ThrowIfPtrIsNull(SDL3_image.IMG_LoadTexture_IO(renderer.Pointer, stream, closeio: true));
        }

        public int Width
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                unsafe
                {
                    return _texture->w;
                }
            }
        }

        public int Height
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                unsafe
                {
                    return _texture->h;
                }
            }
        }

        internal unsafe SDL_Texture* Pointer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return _texture;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ownerRenderer = null;
                }

                unsafe
                {
                    SDL3.SDL_DestroyTexture(_texture);
                }
                disposedValue = true;
            }
        }

        ~Texture()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public enum TextureAccess
    {
        Static = SDL_TextureAccess.SDL_TEXTUREACCESS_STATIC,
        Streaming = SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
        Target = SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET
    }
}
