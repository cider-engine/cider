using Cider.Attributes;
using Cider.Audio;
using Cider.Extensions;
using Cider.Internals;
using SDL;
using System;
using System.Buffers;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Cider.Assets
{
    [SupportedAssetTypes(".wav", ".mp3", ".ogg", ".flac")]
    public class AudioAsset : Asset
    {
#nullable enable
        private Task<Audio>? _cachedAudioLoader = null;
        private CancellationTokenSource _source = new();

        public AudioAsset(string path) : base(path)
        {}

        public Task<Audio> Load(AudioMixer? mixer = null)
        {
            if (_cachedAudioLoader is not null) return _cachedAudioLoader;

            return _cachedAudioLoader = _Load(Path, mixer, _source.Token);

            static async Task<Audio> _Load(string path, AudioMixer? mixer, CancellationToken token)
            {
                if (OperatingSystem.IsBrowser())
                {
                    using var res = await Platform.Browser.Client.GetAsync(Platform.Browser.LocationHref + path, token);

                    res.EnsureSuccessStatusCode();

                    var (context, id) = await Platform.Browser.HttpResponseToIOStreamInterface(res, token);
#pragma warning disable CA1416
                    return await Task.Run(() => LoadInBrowser(context, id, mixer));
#pragma warning restore CA1416
                }

                else return await Task.Run(() => new Audio(path, mixer));
            }

            [SupportedOSPlatform("browser")]
            static unsafe Audio LoadInBrowser(SDL_IOStreamInterface context, int id, AudioMixer? mixer = null)
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
    }

    public class Audio : IDisposable
    {
        private bool disposedValue;
        private unsafe readonly MIX_Audio* _audio;

        internal unsafe MIX_Audio* Pointer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return _audio;
            }
        }

        public unsafe Audio(string path, AudioMixer? mixer = null)
        {
            using var unmanaged = path.ToUnmanagedUtf8();
            _audio = SDLHelpers.ThrowIfPtrIsNull(SDL3_mixer.MIX_LoadAudio(mixer is AudioMixer x ? x.Pointer : null, unmanaged.Pointer, false));
        }

        internal unsafe Audio(SDL_IOStream* stream, AudioMixer? mixer = null)
        {
            _audio = SDLHelpers.ThrowIfPtrIsNull(SDL3_mixer.MIX_LoadAudio_IO(mixer is AudioMixer x ? x.Pointer : null, stream, false, closeio: true));
        }

        public unsafe long FramesToMilliseconds(long frames)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return SDLHelpers.ThrowIfNegative(SDL3_mixer.MIX_AudioFramesToMS(_audio, frames));
        }

        public unsafe long MillisecondsToFrames(long milliseconds)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return SDLHelpers.ThrowIfNegative(SDL3_mixer.MIX_AudioMSToFrames(_audio, milliseconds));
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
                    SDL3_mixer.MIX_DestroyAudio(_audio);
                }
                disposedValue = true;
            }
        }

        ~Audio()
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
