using Cider.Internals;
using System;
using static SDL.SDL3_mixer;

namespace Cider.Audio
{
    public class AudioPlayOptions : PropertyBase
    {
        public int Loops
        {
            get => (int)GetNumberProperty(MIX_PROP_PLAY_LOOPS_NUMBER);
            set => SetNumberProperty(MIX_PROP_PLAY_LOOPS_NUMBER, value);
        }

        public SampleFrame MaxFrame
        {
            get => new(GetNumberProperty(MIX_PROP_PLAY_MAX_FRAME_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_MAX_FRAME_NUMBER, value.Value);
        }

        public TimeSpan MaxTime
        {
            get => TimeSpan.FromMilliseconds(GetNumberProperty(MIX_PROP_PLAY_MAX_MILLISECONDS_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_MAX_MILLISECONDS_NUMBER, long.CreateChecked(value.TotalMilliseconds));
        }

        public SampleFrame StartFrame
        {
            get => new(GetNumberProperty(MIX_PROP_PLAY_START_FRAME_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_START_FRAME_NUMBER, value.Value);
        }

        public TimeSpan StartTime
        {
            get => TimeSpan.FromMilliseconds(GetNumberProperty(MIX_PROP_PLAY_START_MILLISECOND_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_START_MILLISECOND_NUMBER, long.CreateChecked(value.TotalMilliseconds));
        }

        public SampleFrame LoopStartFrame
        {
            get => new(GetNumberProperty(MIX_PROP_PLAY_LOOP_START_FRAME_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_LOOP_START_FRAME_NUMBER, value.Value);
        }

        public TimeSpan LoopStartTime
        {
            get => TimeSpan.FromMilliseconds(GetNumberProperty(MIX_PROP_PLAY_LOOP_START_MILLISECOND_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_LOOP_START_MILLISECOND_NUMBER, long.CreateChecked(value.TotalMilliseconds));
        }

        public SampleFrame FadeInFrames
        {
            get => new(GetNumberProperty(MIX_PROP_PLAY_FADE_IN_FRAMES_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_FADE_IN_FRAMES_NUMBER, value.Value);
        }

        public TimeSpan FadeInTime
        {
            get => TimeSpan.FromMilliseconds(GetNumberProperty(MIX_PROP_PLAY_FADE_IN_MILLISECONDS_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_FADE_IN_MILLISECONDS_NUMBER, long.CreateChecked(value.TotalMilliseconds));
        }

        public float FadeInStartGain
        {
            get => GetFloatProperty(MIX_PROP_PLAY_FADE_IN_START_GAIN_FLOAT);
            set => SetFloatProperty(MIX_PROP_PLAY_FADE_IN_START_GAIN_FLOAT, value);
        }

        public SampleFrame AppendSilenceFrames
        {
            get => new(GetNumberProperty(MIX_PROP_PLAY_APPEND_SILENCE_FRAMES_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_APPEND_SILENCE_FRAMES_NUMBER, value.Value);
        }

        public TimeSpan AppendSilenceTime
        {
            get => TimeSpan.FromMilliseconds(GetNumberProperty(MIX_PROP_PLAY_APPEND_SILENCE_MILLISECONDS_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_APPEND_SILENCE_MILLISECONDS_NUMBER, long.CreateChecked(value.TotalMilliseconds));
        }

        public bool HaltWhenExhausted
        {
            get => GetBooleanProperty(MIX_PROP_PLAY_HALT_WHEN_EXHAUSTED_BOOLEAN);
            set => SetBooleanProperty(MIX_PROP_PLAY_HALT_WHEN_EXHAUSTED_BOOLEAN, value);
        }

        public int StartOrder
        {
            get => int.CreateChecked(GetNumberProperty(MIX_PROP_PLAY_START_ORDER_NUMBER));
            set => SetNumberProperty(MIX_PROP_PLAY_START_ORDER_NUMBER, value);
        }
    }
}
