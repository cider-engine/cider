using Cider.Attributes;
using Cider.Extensions;
using Cider.Internals;
using Cider.Render;
using SDL;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using static SDL.SDL3_ttf;

namespace Cider.Assets
{
    [SupportedAssetTypes(".ttf", ".otf")]
    public class FontAsset : Asset<FontAsset>
    {
#nullable enable
        public const float DefaultPtSize = 18;

        private Task<Font>? _cachedFontLoader = null;
        private CancellationTokenSource _source = new();

        public FontAsset(string path) : base(path)
        {}

        public Task<Font> Load()
        {
            if (_cachedFontLoader is not null) return _cachedFontLoader;

            return _cachedFontLoader = _Load(Path, DefaultPtSize, _source.Token);

            static async Task<Font> _Load(string path, float ptsize, CancellationToken token)
            {
                if (OperatingSystem.IsBrowser())
                {
                    using var res = await Platform.Browser.Browser.Client.GetAsync(Platform.Browser.Browser.LocationHref + path, token);
                    res.EnsureSuccessStatusCode();
                    var (context, id) = await Platform.Browser.Browser.HttpResponseToIOStreamInterface(res, token);
#pragma warning disable CA1416
                    return await Task.Run(() => LoadInBrowser(context, id, ptsize));
#pragma warning restore CA1416
                }

                else return await Task.Run(() => new Font(path, ptsize));
            }

            [SupportedOSPlatform("browser")]
            static unsafe Font LoadInBrowser(SDL_IOStreamInterface context, int id, float ptsize)
            {
                var stream = SDLHelpers.ThrowIfPtrIsNull(SDL3.SDL_OpenIO(&context, (nint)(&id)));
                return new(stream, ptsize);
            }
        }

        public void Unload()
        {
            _source.Cancel();
            _source.Dispose();
            _source = new();
            DisposableHelpers.DisposeAndSetNull(ref _cachedFontLoader);
        }

        public override FontAsset GetThis() => this;
    }

    public class Font : IDisposable
    {
        private bool disposedValue;
        private readonly unsafe TTF_Font* _font;

        internal unsafe TTF_Font* Pointer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return _font;
            }
        }

        public unsafe Font(string path, float ptsize)
        {
            using var unmanaged = path.ToUnmanagedUtf8();
            _font = SDLHelpers.ThrowIfPtrIsNull(TTF_OpenFont(unmanaged.Pointer, ptsize));
        }

        public FontVariant CreateVariant(float fontSize = FontAsset.DefaultPtSize, FontStyleFlags fontStyle = FontStyleFlags.Normal) => new(this)
        {
            FontSize = fontSize,
            FontStyle = fontStyle
        };

        internal unsafe Font(SDL_IOStream* stream, float ptsize)
        {
            _font = SDLHelpers.ThrowIfPtrIsNull(TTF_OpenFontIO(stream, closeio: true, ptsize));
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
            extern static unsafe ref TTF_Font* GetPointer(Font @this);
        }

        ~Font()
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
