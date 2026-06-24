using Cider.Assets;
using Cider.Extensions;
using Cider.Internals;
using SDL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using static SDL.SDL3_ttf;

namespace Cider.Render
{
    [Flags]
    public enum FontStyleFlags : uint
    {
        Normal,
        Bold = 0b1,
        Italic = 0b10,
        Underline = 0b100,
        StrikeThrough = 0b1000
    }

    public class FontVariant : IDisposable
    {
        private bool disposedValue;
        private readonly unsafe TTF_Font* _font;

        public unsafe FontVariant(Font source)
        {
            _font = SDLHelpers.ThrowIfPtrIsNull(TTF_CopyFont(source.Pointer));
        }

        internal unsafe TTF_Font* Pointer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return _font;
            }
        }

        public float FontSize
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                unsafe
                {
                    return SDLHelpers.ThrowIfZero(TTF_GetFontSize(_font));
                }
            }

            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                unsafe
                {
                    SDLHelpers.ThrowIfFalse(TTF_SetFontSize(_font, value));
                }
            }
        }

        public FontStyleFlags FontStyle
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                unsafe
                {
                    return (FontStyleFlags)TTF_GetFontStyle(_font);
                }
            }

            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                unsafe
                {
                    TTF_SetFontStyle(_font, (TTF_FontStyleFlags)value);
                }
            }
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
                    TTF_CloseFont(_font);
                    GetPointer(this) = null;
                }
                disposedValue = true;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(_font))]
            static extern unsafe ref TTF_Font* GetPointer(FontVariant @this);
        }

        public unsafe Surface RenderSolid(ReadOnlySpan<char> text, Color foreground)
        {
            using var unmanaged = text.ToUnmanagedUtf8();
            return new(SDLHelpers.ThrowIfPtrIsNull(TTF_RenderText_Solid(_font, unmanaged.Pointer, length: 0, foreground.AsColor()))); // null-terminated
        }

        public unsafe Surface RenderBlended(ReadOnlySpan<char> text, Color foreground)
        {
            using var unmanaged = text.ToUnmanagedUtf8();
            return new(SDLHelpers.ThrowIfPtrIsNull(TTF_RenderText_Blended(_font, unmanaged.Pointer, length: 0, foreground.AsColor()))); // null-terminated
        }

        public unsafe Surface RenderShaded(ReadOnlySpan<char> text, Color foreground, Color background)
        {
            using var unmanaged = text.ToUnmanagedUtf8();
            return new(SDLHelpers.ThrowIfPtrIsNull(TTF_RenderText_Shaded(_font, unmanaged.Pointer, length: 0, foreground.AsColor(), background.AsColor()))); // null-terminated
        }

        public unsafe Surface RenderLCD(ReadOnlySpan<char> text, Color foreground, Color background)
        {
            using var unmanaged = text.ToUnmanagedUtf8();
            return new(SDLHelpers.ThrowIfPtrIsNull(TTF_RenderText_LCD(_font, unmanaged.Pointer, length: 0, foreground.AsColor(), background.AsColor()))); // null-terminated
        }

        public unsafe (int width, int height) MeasureString(ReadOnlySpan<char> text)
        {
            using var unmanaged = text.ToUnmanagedUtf8();
            int width;
            int height;
            TTF_GetStringSize(_font, unmanaged.Pointer, length: 0, &width, &height); // null-terminated
            return (width, height);
        }

        ~FontVariant()
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
