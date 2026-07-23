using Cider.Internals;
using System;
using static SDL.SDL3_mixer;

namespace Cider.Audio
{
    public class AudioPlayOptions : SDLProperties
    {
        public long LoopTimes
        {
            get => GetNumberProperty(MIX_PROP_PLAY_LOOPS_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_LOOPS_NUMBER, value);
        }

        public long MaxFrame
        {
            get => GetNumberProperty(MIX_PROP_PLAY_MAX_FRAME_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_MAX_FRAME_NUMBER, value);
        }

        public TimeSpan MaxTime
        {
            get => TimeSpan.FromMilliseconds(GetNumberProperty(MIX_PROP_PLAY_MAX_MILLISECONDS_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_MAX_MILLISECONDS_NUMBER, (long)value.TotalMilliseconds);
        }

        public long StartFrame
        {
            get => GetNumberProperty(MIX_PROP_PLAY_START_FRAME_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_START_FRAME_NUMBER, value);
        }

        public TimeSpan StartTime
        {
            get => TimeSpan.FromMilliseconds(GetNumberProperty(MIX_PROP_PLAY_START_MILLISECOND_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_START_MILLISECOND_NUMBER, (long)value.TotalMilliseconds);
        }

        public long LoopStartFrame
        {
            get => GetNumberProperty(MIX_PROP_PLAY_LOOP_START_FRAME_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_LOOP_START_FRAME_NUMBER, value);
        }

        public TimeSpan LoopStartTime
        {
            get => TimeSpan.FromMilliseconds(GetNumberProperty(MIX_PROP_PLAY_LOOP_START_MILLISECOND_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_LOOP_START_MILLISECOND_NUMBER, (long)value.TotalMilliseconds);
        }

        public long FadeInFrames
        {
            get => GetNumberProperty(MIX_PROP_PLAY_FADE_IN_FRAMES_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_FADE_IN_FRAMES_NUMBER, value);
        }

        public TimeSpan FadeInTime
        {
            get => TimeSpan.FromMilliseconds(GetNumberProperty(MIX_PROP_PLAY_FADE_IN_MILLISECONDS_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_FADE_IN_MILLISECONDS_NUMBER, (long)value.TotalMilliseconds);
        }

        public float FadeInStartGain
        {
            get => GetFloatProperty(MIX_PROP_PLAY_FADE_IN_START_GAIN_FLOAT);
            set => SetFloatProperty(MIX_PROP_PLAY_FADE_IN_START_GAIN_FLOAT, value);
        }

        public long AppendSilenceFrames
        {
            get => GetNumberProperty(MIX_PROP_PLAY_APPEND_SILENCE_FRAMES_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_APPEND_SILENCE_FRAMES_NUMBER, value);
        }

        public TimeSpan AppendSilenceTime
        {
            get => TimeSpan.FromMilliseconds(GetNumberProperty(MIX_PROP_PLAY_APPEND_SILENCE_MILLISECONDS_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_APPEND_SILENCE_MILLISECONDS_NUMBER, (long)value.TotalMilliseconds);
        }

        public bool HaltWhenExhausted
        {
            get => GetBooleanProperty(MIX_PROP_PLAY_HALT_WHEN_EXHAUSTED_BOOLEAN);
            set => SetBooleanProperty(MIX_PROP_PLAY_HALT_WHEN_EXHAUSTED_BOOLEAN, value);
        }

        public long StartOrder
        {
            get => GetNumberProperty(MIX_PROP_PLAY_START_ORDER_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_START_ORDER_NUMBER, value);
        }
    }
}
