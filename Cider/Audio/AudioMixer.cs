using Cider.Internals;
using SDL;
using System;
using System.Collections;
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
        public static ICollection<AudioMixer> AllMixers => MixerDictionary.Values;
        public static AudioMixer DefaultPlayback { get; } = new((uint)SDL3.SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK);

        private unsafe readonly MIX_Mixer* _mixer;
        private bool disposedValue;
        internal readonly ConcurrentDictionary<nint, AudioTrack> TrackDictionary = new();

        internal unsafe MIX_Mixer* Pointer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return _mixer;
            }
        }

        public ICollection<AudioTrack> AllTracks
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return TrackDictionary.Values;
            }
        }

        internal unsafe AudioMixer(uint id)
        {
            _mixer = SDLHelpers.ThrowIfPtrIsNull(MIX_CreateMixerDevice((SDL_AudioDeviceID)id, null));
            if (!MixerDictionary.TryAdd((nint)_mixer, this)) throw new CiderGameException();
        }

        public unsafe float FrequencyRatio
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return SDLHelpers.ThrowIfZero(MIX_GetMixerFrequencyRatio(_mixer));
            }
            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                SDLHelpers.ThrowIfFalse(MIX_SetMixerFrequencyRatio(_mixer, value));
            }
        }

        public unsafe float Gain
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return SDLHelpers.ThrowIfZero(MIX_GetMixerGain(_mixer));
            }
            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                SDLHelpers.ThrowIfFalse(MIX_SetMixerGain(_mixer, value));
            }
        }

        public void Play(AudioClip audio)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            unsafe
            {
                MIX_PlayAudio(_mixer, audio.Pointer);
            }
        }

        public void StopAllTracks(TimeSpan fadeOutTime = default)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            unsafe
            {
                MIX_StopAllTracks(_mixer, (long)fadeOutTime.TotalMilliseconds);
            }
        }

        public AudioTrack CreateTrack()
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            var track = new AudioTrack(this);
            unsafe
            {
                if (!TrackDictionary.TryAdd((nint)track.Pointer, track))
                    throw new CiderGameException();
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
                }

                unsafe
                {
                    MIX_DestroyMixer(_mixer);
                    GetPointer(this) = null;
                }
                disposedValue = true;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(_mixer))]
            static extern unsafe ref MIX_Mixer* GetPointer(AudioMixer @this);
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
}
