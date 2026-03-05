using Cider.Attributes;
using Cider.Internals;
using Cider.Render;
using SDL;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Cider.Assets
{
    [SupportedAssetTypes(".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp", ".ico", ".svg")]
    public class TextureAsset : Asset
    {
#nullable enable
        private Task<Surface>? _cachedSurfaceLoader = null;
        private CancellationTokenSource _surfaceTokenSource = new();

        public TextureAsset(string path) : base(path)
        { }

        public Task<Surface> LoadSurface()
        {
            if (_cachedSurfaceLoader is not null) return _cachedSurfaceLoader;

            return _cachedSurfaceLoader = _Load(Path, _surfaceTokenSource.Token);

            static async Task<Surface> _Load(string path, CancellationToken token)
            {
                if (OperatingSystem.IsBrowser())
                {
                    using var res = await Platform.Browser.Client.GetAsync(Platform.Browser.LocationHref + path, token);
                    res.EnsureSuccessStatusCode();
                    var (context, id) = await Platform.Browser.HttpResponseToIOStreamInterface(res, token);
#pragma warning disable CA1416
                    return await Task.Run(() => LoadInBrowser(context, id));
#pragma warning restore CA1416
                }

                else return await Task.Run(() => new Surface(path));
            }

            [SupportedOSPlatform("browser")]
            static unsafe Surface LoadInBrowser(SDL_IOStreamInterface context, int id)
            {
                var stream = SDLHelpers.ThrowIfPtrIsNull(SDL3.SDL_OpenIO(&context, (nint)(&id)));
                return new(stream);
            }
        }

        public Task<Texture> LoadTexture(Renderer renderer)
        {
            if (renderer.Textures.TryGetValue(this, out var x)) return x.task;

            var source = new CancellationTokenSource();

            var task = _Load(Path, renderer, source.Token);

            renderer.Textures[this] = (source, task);

            return task;

            static async Task<Texture> _Load(string path, Renderer renderer, CancellationToken token)
            {
                if (OperatingSystem.IsBrowser())
                {
                    using var res = await Platform.Browser.Client.GetAsync(Platform.Browser.LocationHref + path, token);
                    res.EnsureSuccessStatusCode();
                    var (context, id) = await Platform.Browser.HttpResponseToIOStreamInterface(res, token);
#pragma warning disable CA1416
                    return await Task.Run(() => LoadInBrowser(context, id, renderer));
#pragma warning restore CA1416
                }

                else return await Task.Run(() => new Texture(renderer, path));
            }

            [SupportedOSPlatform("browser")]
            static unsafe Texture LoadInBrowser(SDL_IOStreamInterface context, int id, Renderer renderer)
            {
                var stream = SDLHelpers.ThrowIfPtrIsNull(SDL3.SDL_OpenIO(&context, (nint)(&id)));
                return new(renderer, stream);
            }
        }

        public void UnloadSurface()
        {
            _surfaceTokenSource.Cancel();
            _surfaceTokenSource.Dispose();
            _surfaceTokenSource = new();
            DisposableHelpers.DisposeAndSetNull(ref _cachedSurfaceLoader);
        }

        public void UnloadTexture(Renderer renderer)
        {
            if (renderer.Textures.TryGetValue(this, out var x))
            {
                x.source.Cancel();
                x.source.Dispose();
                x.task.ContinueWith(static task =>
                {
                    if (task.IsCompletedSuccessfully) task.Result.Dispose();
                });
                renderer.Textures.Remove(this);
            }
        }
    }
}
