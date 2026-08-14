using Cider.Attributes;
using Cider.Internals;
using Cider.Render;
using SDL;
using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Cider.Assets
{
    [SupportedAssetTypes(".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp", ".ico", ".svg")]
    public class TextureAsset : Asset<TextureAsset>
    {
        private Task<Surface>? _cachedSurfaceLoader = null;
        private CancellationTokenSource _surfaceTokenSource = new();

        public TextureAsset(string path) : base(path)
        { }

        public Task<Surface> LoadSurfaceAsync()
        {
            if (_cachedSurfaceLoader is not null) return _cachedSurfaceLoader;

            return _cachedSurfaceLoader = _Load(Path, _surfaceTokenSource.Token);

            static async Task<Surface> _Load(string path, CancellationToken token)
            {
                if (OperatingSystem.IsBrowser())
                {
                    using var res = await Platform.Browser.Browser.Client.GetAsync(Platform.Browser.Browser.LocationHref + path, token);
                    res.EnsureSuccessStatusCode();
                    var (context, id) = await Platform.Browser.Browser.HttpResponseToIOStreamInterface(res, token);
                    return LoadInBrowser(context, id);
                }

                else return await Task.Run(() => new Surface(path));
            }

            [SupportedOSPlatform("browser")]
            static unsafe Surface LoadInBrowser(SDL_IOStreamInterface context, int id)
            {
                var stream = SDLHelpers.ThrowIfPtrIsNull(SDL3.SDL_OpenIO(&context, id));
                return new(stream);
            }
        }

        public Task<Texture> LoadTextureAsync(Renderer renderer)
        {
            if (renderer.Textures.TryGetValue(this, out var x)) return x;

            var task = _Load(this, renderer);

            renderer.Textures[this] = task;

            return task;

            static async Task<Texture> _Load(TextureAsset asset, Renderer renderer)
            {
                return new(renderer, await asset.LoadSurfaceAsync());
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
                x.ContinueWith(static task =>
                {
                    if (task.IsCompletedSuccessfully) task.Result.Dispose();
                }, Game.GetTaskScheduler());
                renderer.Textures.Remove(this);
            }
        }

        public override TextureAsset GetThis() => this;
    }
}
