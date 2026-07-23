using Cider.Attributes;
using Cider.Audio;
using Cider.Internals;
using SDL;
using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Cider.Assets
{
    [SupportedAssetTypes(".wav", ".mp3", ".ogg", ".flac")]
    public class AudioAsset : Asset<AudioAsset>
    {
#nullable enable
        private Task<AudioClip>? _cachedAudioLoader = null;
        private CancellationTokenSource _source = new();

        public AudioAsset(string path) : base(path)
        {}

        public Task<AudioClip> Load(AudioMixer? mixer = null)
        {
            if (_cachedAudioLoader is not null) return _cachedAudioLoader;

            return _cachedAudioLoader = _Load(Path, mixer, _source.Token);

            static async Task<AudioClip> _Load(string path, AudioMixer? mixer, CancellationToken token)
            {
                if (OperatingSystem.IsBrowser())
                {
                    using var res = await Platform.Browser.Browser.Client.GetAsync(Platform.Browser.Browser.LocationHref + path, token);

                    res.EnsureSuccessStatusCode();

                    var (context, id) = await Platform.Browser.Browser.HttpResponseToIOStreamInterface(res, token);
#pragma warning disable CA1416
                    return await Task.Run(() => LoadInBrowser(context, id, mixer));
#pragma warning restore CA1416
                }

                else return await Task.Run(() => new AudioClip(path, mixer));
            }

            [SupportedOSPlatform("browser")]
            static unsafe AudioClip LoadInBrowser(SDL_IOStreamInterface context, int id, AudioMixer? mixer = null)
            {
                var stream = SDLHelpers.ThrowIfPtrIsNull(SDL3.SDL_OpenIO(&context, (nint)(&id)));
                return new(stream, mixer);
            }
        }

        public void Unload()
        {
            _source.Cancel();
            _source.Dispose();
            _source = new();
            DisposableHelpers.DisposeAndSetNull(ref _cachedAudioLoader);
        }

        public override AudioAsset GetThis() => this;
    }
}