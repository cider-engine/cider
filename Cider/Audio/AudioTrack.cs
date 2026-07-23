using Cider.Internals;
using SDL;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static SDL.SDL3_mixer;

namespace Cider.Audio
{
    public delegate void AudioTrackStoppedEventHandler(AudioMixer mixer, AudioTrack track);

    public class AudioTrack : IDisposable
    {
        private readonly unsafe MIX_Track* _track;
        private bool disposedValue;
        private AudioMixer ownerMixer;

        public event AudioTrackStoppedEventHandler Stopped;

        public AudioMixer OwnerMixer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return ownerMixer;
            }
        }

        internal unsafe MIX_Track* Pointer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return _track;
            }
        }

        public unsafe AudioTrack(AudioMixer mixer)
        {
            ownerMixer = mixer;
            _track = SDLHelpers.ThrowIfPtrIsNull(MIX_CreateTrack(mixer.Pointer));

            delegate* unmanaged[Cdecl]<nint, MIX_Track*, void> callback = &TrackStoppedCallback;

            SDLHelpers.ThrowIfFalse(SetTrackStoppedCallback(_track, (nint)callback, (nint)mixer.Pointer));
        }
#nullable enable
        public AudioClip? TrackAudio
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return field;
            }
            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                unsafe
                {
                    SDLHelpers.ThrowIfFalse(MIX_SetTrackAudio(_track, value is AudioClip audio ? audio.Pointer : null));
                    field = value;
                }
            }
        }

        public unsafe float FrequencyRatio
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return SDLHelpers.ThrowIfZero(MIX_GetTrackFrequencyRatio(_track));
            }
            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                SDLHelpers.ThrowIfFalse(MIX_SetTrackFrequencyRatio(_track, value));
            }
        }

        public unsafe float Gain
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return SDLHelpers.ThrowIfZero(MIX_GetTrackGain(_track));
            }
            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                SDLHelpers.ThrowIfFalse(MIX_SetTrackGain(_track, value));
            }
        }

        public unsafe long PlaybackPositionFrames
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return SDLHelpers.ThrowIfNegative(MIX_GetTrackPlaybackPosition(_track));
            }
            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                SDLHelpers.ThrowIfFalse(MIX_SetTrackPlaybackPosition(_track, value));
            }
        }

        public TimeSpan PlaybackPositionTime
        {
            get => FramesToTimeSpan(PlaybackPositionFrames);
            set => PlaybackPositionFrames = TimeSpanToFrames(value);
        }

        public unsafe TimeSpan FramesToTimeSpan(long frames)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return TimeSpan.FromMilliseconds(SDLHelpers.ThrowIfNegative(MIX_TrackFramesToMS(_track, frames)));
        }

        public unsafe long TimeSpanToFrames(TimeSpan timeSpan)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return SDLHelpers.ThrowIfNegative(MIX_TrackMSToFrames(_track, (long)timeSpan.TotalMilliseconds));
        }

        public void Play(AudioPlayOptions? options = null)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            unsafe
            {
                SDLHelpers.ThrowIfFalse(MIX_PlayTrack(_track, options is AudioPlayOptions x ? x.Pointer : default));
            }
        }

        public void Pause()
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            unsafe
            {
                SDLHelpers.ThrowIfFalse(MIX_PauseTrack(_track));
            }
        }

        public void Stop(long fadeOutFrames)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            unsafe
            {
                SDLHelpers.ThrowIfFalse(MIX_StopTrack(_track, fadeOutFrames));
            }
        }

        public void Stop(TimeSpan fadeOutTime = default)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            unsafe
            {
                SDLHelpers.ThrowIfFalse(MIX_StopTrack(_track, TimeSpanToFrames(fadeOutTime)));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    unsafe
                    {
                        ownerMixer.TrackDictionary.TryRemove((nint)_track, out _);
                    }
                    ownerMixer = null;
                    TrackAudio = null;
                }

                unsafe
                {
                    MIX_DestroyTrack(_track);
                    GetPointer(this) = null;
                }
                disposedValue = true;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(_track))]
            static extern unsafe ref MIX_Track* GetPointer(AudioTrack @this);
        }

        ~AudioTrack()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // WASM的另一个函数签名不匹配问题，类似于SDL_WindowProperties，我乱改函数签名的时候发现把回调的类型从函数指针改成nint就能跑了，我也不知道为什么
        [DllImport("SDL3_mixer", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = nameof(MIX_SetTrackStoppedCallback))]
        unsafe static extern SDLBool SetTrackStoppedCallback(MIX_Track* track, nint cb, nint userdata);

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static unsafe void TrackStoppedCallback(nint mixerPtr, MIX_Track* trackPtr)
        {
            if (AudioMixer.MixerDictionary.TryGetValue(mixerPtr, out var mixer) && mixer.TrackDictionary.TryGetValue((nint)trackPtr, out var track))
                track.Stopped?.Invoke(mixer, track);

            else
                Debug.Assert(false);
        }
    }
}
