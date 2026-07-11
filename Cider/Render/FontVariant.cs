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
        Normal = TTF_FontStyleFlags.TTF_STYLE_NORMAL,
        Bold = TTF_FontStyleFlags.TTF_STYLE_BOLD,
        Italic = TTF_FontStyleFlags.TTF_STYLE_ITALIC,
        Underline = TTF_FontStyleFlags.TTF_STYLE_UNDERLINE,
        StrikeThrough = TTF_FontStyleFlags.TTF_STYLE_STRIKETHROUGH
    }

    public enum FontLineJoin
    {
        Round = 0,
        Bevel = 1,
        Miter = 2,
        MiterFixed= 3
    }

    public enum FontLineCap
    {
        Butt = 0,
        Round,
        Square
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

        public bool IsSDFEnabled
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                unsafe
                {
                    return TTF_GetFontSDF(_font);
                }
            }

            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                unsafe
                {
                    SDLHelpers.ThrowIfFalse(TTF_SetFontSDF(_font, value));
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

        public int Outline
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                unsafe
                {
                    return TTF_GetFontOutline(_font);
                }
            }

            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                unsafe
                {
                    TTF_SetFontOutline(_font, value);
                }
            }
        }

        public unsafe FontLineJoin OutlineLineJoin
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                fixed (byte* unmanaged = TTF_PROP_FONT_OUTLINE_LINE_JOIN_NUMBER)
                    return (FontLineJoin)SDL3.SDL_GetNumberProperty(GetProperties(), unmanaged, default);
            }

            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                fixed (byte* unmanaged = TTF_PROP_FONT_OUTLINE_LINE_JOIN_NUMBER)
                    SDLHelpers.ThrowIfFalse(SDL3.SDL_SetNumberProperty(GetProperties(), unmanaged, (long)value));
            }
        }

        public unsafe FontLineCap OutlineLineCap
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                fixed (byte* unmanaged = TTF_PROP_FONT_OUTLINE_LINE_CAP_NUMBER)
                    return (FontLineCap)SDL3.SDL_GetNumberProperty(GetProperties(), unmanaged, default);
            }

            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                fixed (byte* unmanaged = TTF_PROP_FONT_OUTLINE_LINE_CAP_NUMBER)
                    SDLHelpers.ThrowIfFalse(SDL3.SDL_SetNumberProperty(GetProperties(), unmanaged, (long)value));
            }
        }

        public unsafe long OutlineMiterLimit
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                fixed (byte* unmanaged = TTF_PROP_FONT_OUTLINE_MITER_LIMIT_NUMBER)
                    return SDL3.SDL_GetNumberProperty(GetProperties(), unmanaged, default);
            }

            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                fixed (byte* unmanaged = TTF_PROP_FONT_OUTLINE_MITER_LIMIT_NUMBER)
                    SDLHelpers.ThrowIfFalse(SDL3.SDL_SetNumberProperty(GetProperties(), unmanaged, value));
            }
        }

        public unsafe int CharSpacing
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return TTF_GetFontCharSpacing(_font);
            }

            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                SDLHelpers.ThrowIfFalse(TTF_SetFontCharSpacing(_font, value));
            }
        }

        unsafe SDL_PropertiesID GetProperties()
        {
            var properties = TTF_GetFontProperties(_font);
            SDLHelpers.ThrowIfZero((uint)properties);
            return properties;
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
