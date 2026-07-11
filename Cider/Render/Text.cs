using Cider.Extensions;
using Cider.Internals;
using SDL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Cider.Render
{
    public class Text : IDisposable
    {
        private bool disposedValue;
        private readonly unsafe TTF_Text* _text;

        internal unsafe TTF_Text* Pointer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return _text;
            }
        }

        public ITextEngine OwnerTextEngine { get; }

        public unsafe int Width
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);

                int width;
                SDLHelpers.ThrowIfFalse(SDL3_ttf.TTF_GetTextSize(_text, &width, null));

                return width;
            }
        }

        public unsafe int Height
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);

                int height;
                SDLHelpers.ThrowIfFalse(SDL3_ttf.TTF_GetTextSize(_text, null, &height));

                return height;
            }
        }

        public unsafe Color Color
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);

                byte r, g, b, a;
                SDLHelpers.ThrowIfFalse(SDL3_ttf.TTF_GetTextColor(_text, &r, &g, &b, &a));

                return Color.FromArgb(a, r, g, b);
            }

            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);

                SDLHelpers.ThrowIfFalse(SDL3_ttf.TTF_SetTextColor(_text, value.R, value.G, value.B, value.A));
            }
        }

        public unsafe void SetContent(ReadOnlySpan<char> text)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);

            using var unmanaged = text.ToUnmanagedUtf8();

            SDLHelpers.ThrowIfFalse(SDL3_ttf.TTF_SetTextString(_text, unmanaged.Pointer, 0));
        }

        public unsafe Text(ITextEngine engine, FontVariant variant, ReadOnlySpan<char> text)
        {
            using var unmanaged = text.ToUnmanagedUtf8();
            _text = SDLHelpers.ThrowIfPtrIsNull(SDL3_ttf.TTF_CreateText(engine.Pointer, variant.Pointer, unmanaged.Pointer, 0));

            OwnerTextEngine = engine;
        }

        public void Render(float x, float y)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            OwnerTextEngine.RenderTo(this, x, y);
        }

        public unsafe void Measure(out int width, out int height)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);

            fixed (int* w = &width, h = &height)
                SDLHelpers.ThrowIfFalse(SDL3_ttf.TTF_GetTextSize(_text, w, h));
        }

        public unsafe void Update()
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);

            SDLHelpers.ThrowIfFalse(SDL3_ttf.TTF_UpdateText(_text));
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
                    SDL3_ttf.TTF_DestroyText(_text);
                    GetPointer(this) = null;
                }
                disposedValue = true;
            }


            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(_text))]
            extern static unsafe ref TTF_Text* GetPointer(Text @this);
        }

        ~Text()
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
