using Cider.Extensions;
using Cider.Internals;
using SDL;
using System;
using System.Drawing;

namespace Cider.Render
{
    public class Surface : IDisposable
    {
        private bool disposedValue;
        private unsafe readonly SDL_Surface* _surface;

        internal unsafe SDL_Surface* Pointer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return _surface;
            }
        }

        public unsafe Surface(int width, int height)
        {
            _surface = SDLHelpers.ThrowIfPtrIsNull(SDL3.SDL_CreateSurface(width, height, SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888));
        }

        public unsafe Surface(string path)
        {
            using var unmanaged = path.ToUnmanagedUtf8();
            _surface = SDLHelpers.ThrowIfPtrIsNull(SDL3_image.IMG_Load(unmanaged.Pointer));
        }

        internal unsafe Surface(SDL_Surface* surface)
        {
            _surface = surface;
        }

        internal unsafe Surface(SDL_IOStream* stream)
        {
            _surface = SDLHelpers.ThrowIfPtrIsNull(SDL3_image.IMG_Load_IO(stream, closeio: true));
        }

        public unsafe void Clear(Color color)
        {
            SDLHelpers.ThrowIfFalse(SDL3.SDL_ClearSurface(_surface, color.R, color.G, color.B, color.A));
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
                    SDL3.SDL_DestroySurface(_surface);
                }
                disposedValue = true;
            }
        }

        ~Surface()
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
