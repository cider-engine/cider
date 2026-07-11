using Cider.Attributes;
using DotTiled;
using DotTiled.Serialization;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cider.Assets
{
    [SupportedAssetTypes(".tmx", ".tmj")]
    public class TileMapAsset : Asset<TileMapAsset>
    {
#nullable enable
        private Task<Map>? _cachedTileMapLoader = null;
        private CancellationTokenSource _source = new();

        public TileMapAsset(string path) : base(path)
        {
        }

        public Task<Map> Load()
        {
            if (_cachedTileMapLoader is not null) return _cachedTileMapLoader;

            return _cachedTileMapLoader = _Load(Path, _source.Token);

            static async Task<Map> _Load(string path, CancellationToken token)
            {
                return await Loader.DefaultWith(new TileMapResourceReader(token)).LoadMapAsync(path);
            }
        }

        public void Unload()
        {
            _source.Cancel();
            _source.Dispose();
            _source = new();
            _cachedTileMapLoader = null;
        }

        public override TileMapAsset GetThis() => this;

        class TileMapResourceReader(CancellationToken token) : IResourceReader
        {
            public async Task<string> ReadAsync(string resourcePath)
            {
                if (OperatingSystem.IsBrowser())
                {
                    using var res = await Platform.Browser.Browser.Client.GetAsync(Platform.Browser.Browser.LocationHref + resourcePath, token);
                    res.EnsureSuccessStatusCode();
                    return await res.Content.ReadAsStringAsync(token);
                }

                else
                {
                    return await File.ReadAllTextAsync(resourcePath, token);
                }
            }
        }
    }
}
