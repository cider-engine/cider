using Cider.Extensions;
using Cider.Internals;
using SDL;
using System;
using System.Runtime.CompilerServices;

#nullable enable
namespace Cider.Audio
{
    public class AudioClip : IDisposable
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

        public unsafe AudioClip(string path, AudioMixer? mixer = null)
        {
            using var unmanaged = path.ToUnmanagedUtf8();
            _audio = SDLHelpers.ThrowIfPtrIsNull(SDL3_mixer.MIX_LoadAudio(mixer?.Pointer, unmanaged.Pointer, false));
        }

        internal unsafe AudioClip(SDL_IOStream* stream, AudioMixer? mixer = null)
        {
            _audio = SDLHelpers.ThrowIfPtrIsNull(SDL3_mixer.MIX_LoadAudio_IO(mixer?.Pointer , stream, false, closeio: true));
        }

        public unsafe TimeSpan FramesToTimeSpan(long frames)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return TimeSpan.FromMilliseconds(SDLHelpers.ThrowIfNegative(SDL3_mixer.MIX_AudioFramesToMS(_audio, frames)));
        }

        public unsafe long TimeSpanToFrames(TimeSpan timeSpan)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return SDLHelpers.ThrowIfNegative(SDL3_mixer.MIX_AudioMSToFrames(_audio, (long)timeSpan.TotalMilliseconds));
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
                    GetPointer(this) = null;
                }
                disposedValue = true;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(_audio))]
            extern static unsafe ref MIX_Audio* GetPointer(AudioClip @this);
        }

        ~AudioClip()
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