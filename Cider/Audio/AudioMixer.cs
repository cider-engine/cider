using Cider.Internals;
using SDL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using static SDL.SDL3_mixer;

namespace Cider.Audio
{
    public class AudioMixer : IDisposable
    {
        internal readonly static ConcurrentDictionary<nint, AudioMixer> MixerDictionary = new();
        public static IReadOnlyCollection<AudioMixer> AllMixers => (IReadOnlyCollection<AudioMixer>)MixerDictionary.Values;
        public static AudioMixer DefaultPlayback { get; } = new(((uint)SDL3.SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK));

        private unsafe readonly MIX_Mixer* _mixer;
        private bool disposedValue;
        internal ConcurrentDictionary<nint, AudioTrack> TrackDictionary = new();

        internal unsafe MIX_Mixer* Pointer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return _mixer;
            }
        }

        public IReadOnlyCollection<AudioTrack> AllTracks
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return (IReadOnlyCollection<AudioTrack>)TrackDictionary.Values;
            }
        }

        internal unsafe AudioMixer(uint id)
        {
            _mixer = SDLHelpers.ThrowIfPtrIsNull(MIX_CreateMixerDevice((SDL_AudioDeviceID)id, null));
            if (!MixerDictionary.TryAdd((nint)_mixer, this)) throw new Exception();
        }

        public void Play(Assets.Audio audio)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            unsafe
            {
                MIX_PlayAudio(_mixer, audio.Pointer);
            }
        }

        public AudioTrack CreateTrack()
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            var track = new AudioTrack(this);
            unsafe
            {
                if (!TrackDictionary.TryAdd((nint)track.Pointer, track))
                    throw new Exception();
            }
            return track;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    unsafe
                    {
                        MixerDictionary.TryRemove((nint)_mixer, out _);
                    }
                    foreach (var track in TrackDictionary.Values)
                        track.Dispose();

                    TrackDictionary.Clear();
                    TrackDictionary = null;
                }

                unsafe
                {
                    MIX_DestroyMixer(_mixer);
                }
                disposedValue = true;
            }
        }

        ~AudioMixer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class AudioTrack : IDisposable
    {
        private readonly unsafe MIX_Track* _track;
        private bool disposedValue;
        private AudioMixer ownerMixer;
        private EventHandler<AudioTrack, EventArgs> _stopped;

        [UnsupportedOSPlatform("browser")] // MIX_SetTrackStoppedCallback一旦调用就会崩溃，原因不明
        public event EventHandler<AudioTrack, EventArgs> Stopped
        {
            add
            {
                if (OperatingSystem.IsBrowser()) throw new PlatformNotSupportedException();
                _stopped += value;
            }
            remove
            {
                if (OperatingSystem.IsBrowser()) throw new PlatformNotSupportedException();
                _stopped -= value;
            }
        }

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

            if (!OperatingSystem.IsBrowser()) // MIX_SetTrackStoppedCallback一旦调用就会崩溃，原因不明
                SDLHelpers.ThrowIfFalse(MIX_SetTrackStoppedCallback(_track, &TrackStoppedCallback, (nint)mixer.Pointer));
        }
#nullable enable
        public Assets.Audio? TrackAudio
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
                    SDLHelpers.ThrowIfFalse(MIX_SetTrackAudio(_track, value is Assets.Audio audio ? audio.Pointer : null));
                    field = value;
                }
            }
        }

        public unsafe long FramesToMilliseconds(long frames)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return SDLHelpers.ThrowIfNegative(MIX_TrackFramesToMS(_track, frames));
        }

        public unsafe long MillisecondsToFrames(long milliseconds)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return SDLHelpers.ThrowIfNegative(MIX_TrackMSToFrames(_track, milliseconds));
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
                SDLHelpers.ThrowIfFalse(MIX_StopTrack(_track, MillisecondsToFrames((long)fadeOutTime.TotalMilliseconds)));
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
                }
                disposedValue = true;
            }
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

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static unsafe void TrackStoppedCallback(nint mixerPtr, MIX_Track* trackPtr)
        {
            if (AudioMixer.MixerDictionary.TryGetValue(mixerPtr, out var mixer) && mixer.TrackDictionary.TryGetValue((nint)trackPtr, out var track))
                track._stopped?.Invoke(track, EventArgs.Empty);

            else
                Debug.Assert(false);
        }
    }

    public class AudioPlayOptions : SDLProperties
    {
        public long LoopTimes
        {
            get => GetNumberProperty(MIX_PROP_PLAY_LOOPS_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_LOOPS_NUMBER, value);
        }

        public long PlayMaxFrame
        {
            get => GetNumberProperty(MIX_PROP_PLAY_MAX_FRAME_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_MAX_FRAME_NUMBER, value);
        }

        public long PlayMaxMilliseconds
        {
            get => GetNumberProperty(MIX_PROP_PLAY_MAX_MILLISECONDS_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_MAX_MILLISECONDS_NUMBER, value);
        }

        public long PlayStartFrame
        {
            get => GetNumberProperty(MIX_PROP_PLAY_START_FRAME_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_START_FRAME_NUMBER, value);
        }

        public long PlayStartMillisecond
        {
            get => GetNumberProperty(MIX_PROP_PLAY_START_MILLISECOND_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_START_MILLISECOND_NUMBER, value);
        }

        public long PlayLoopStartFrame
        {
            get => GetNumberProperty(MIX_PROP_PLAY_LOOP_START_FRAME_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_LOOP_START_FRAME_NUMBER, value);
        }

        public long PlayLoopStartMillisecond
        {
            get => GetNumberProperty(MIX_PROP_PLAY_LOOP_START_MILLISECOND_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_LOOP_START_MILLISECOND_NUMBER, value);
        }

        public long PlayFadeInFrames
        {
            get => GetNumberProperty(MIX_PROP_PLAY_FADE_IN_FRAMES_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_FADE_IN_FRAMES_NUMBER, value);
        }

        public long PlayFadeInMilliseconds
        {
            get => GetNumberProperty(MIX_PROP_PLAY_FADE_IN_MILLISECONDS_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_FADE_IN_MILLISECONDS_NUMBER, value);
        }

        public float PlayFadeInStartGain
        {
            get => GetFloatProperty(MIX_PROP_PLAY_FADE_IN_START_GAIN_FLOAT);
            set => SetFloatProperty(MIX_PROP_PLAY_FADE_IN_START_GAIN_FLOAT, value);
        }

        public long PlayAppendSilenceFrames
        {
            get => GetNumberProperty(MIX_PROP_PLAY_APPEND_SILENCE_FRAMES_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_APPEND_SILENCE_FRAMES_NUMBER, value);
        }

        public long PlayAppendSilenceMilliseconds
        {
            get => GetNumberProperty(MIX_PROP_PLAY_APPEND_SILENCE_MILLISECONDS_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_APPEND_SILENCE_MILLISECONDS_NUMBER, value);
        }
    }
}
