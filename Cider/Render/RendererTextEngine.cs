using Cider.Internals;
using SDL;
using System;
using System.Runtime.CompilerServices;

namespace Cider.Render
{
    public class RendererTextEngine : ITextEngine
    {
        private bool disposedValue;
        private unsafe readonly TTF_TextEngine* _engine;

        unsafe TTF_TextEngine* ITextEngine.Pointer => _engine;

        public Renderer OwnerRenderer { get; }

        public unsafe RendererTextEngine(Renderer renderer)
        {
            _engine = SDLHelpers.ThrowIfPtrIsNull(SDL3_ttf.TTF_CreateRendererTextEngine(renderer.Pointer));
            OwnerRenderer = renderer;
        }

        public void RenderTo(Text text, float x, float y)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            if (!ReferenceEquals(this, text.OwnerTextEngine)) throw new InvalidOperationException("The text is not rendered from owner text engine.");
            unsafe
            {
                var offset = OwnerRenderer.Camera2D.OffsetPosition;
                SDLHelpers.ThrowIfFalse(SDL3_ttf.TTF_DrawRendererText(text.Pointer, x - offset.X, y - offset.Y));
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                unsafe
                {
                    SDL3_ttf.TTF_DestroyRendererTextEngine(_engine);
                    GetPointer(this) = null;
                }
                disposedValue = true;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(_engine))]
            static extern unsafe ref TTF_TextEngine* GetPointer(RendererTextEngine @this);
        }

        ~RendererTextEngine()
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
