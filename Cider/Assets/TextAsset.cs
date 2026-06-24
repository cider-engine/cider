using Cider.Attributes;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cider.Assets
{
    [SupportedAssetTypes(".txt")]
    public class TextAsset : Asset<TextAsset>
    {
#nullable enable
        private Task<string>? _cachedTextLoader = null;
        private CancellationTokenSource _source = new();

        public TextAsset(string path) : base(path)
        {
        }

        public Task<string> Load()
        {
            if (_cachedTextLoader is not null) return _cachedTextLoader;

            return _cachedTextLoader = _Load(Path, _source.Token);

            static async Task<string> _Load(string path, CancellationToken token)
            {
                if (OperatingSystem.IsBrowser())
                {
                    using var res = await Platform.Browser.Browser.Client.GetAsync(Platform.Browser.Browser.LocationHref + path, token);
                    res.EnsureSuccessStatusCode();
                    return await res.Content.ReadAsStringAsync(token);
                }

                else
                {
                    return await File.ReadAllTextAsync(path, token);
                }
            }
        }

        public void Unload()
        {
            _source.Cancel();
            _source.Dispose();
            _source = new();
            _cachedTextLoader = null;
        }

        public override TextAsset GetThis() => this;
    }
}
