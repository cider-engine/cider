using System;
using System.Numerics;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;

#nullable enable
namespace Cider.Animation
{

    public static class Tween
    {
        public static ITween<T> Create<T>(T initialValue, T endValue, Action<T> valueSetter, TimeSpan duration) where T : INumber<T>
        {
            var tween = new Tween<T>(initialValue, endValue, duration);
            tween.ValueChanged += valueSetter;
            return tween;
        }

        public static ITween<Vector2> Create(Vector2 initialValue, Vector2 endValue, Action<Vector2> valueSetter, TimeSpan duration)
        {
            var tween = new TweenVector2(initialValue, endValue, duration);
            tween.ValueChanged += valueSetter;
            return tween;
        }
    }

    public interface ITween<T>
    {
        T Start { get; }

        T End { get; }

        TimeSpan Duration { get; }

        EasingFunction Easing { get; set; }

        Task Task { get; }

        bool Loop { get; set; }

        PlaybackDirection Direction { get; set; }

        bool IsPlaying { get; }

        bool IsCompleted { get; }

        event Action<T> ValueChanged;

        event Action Completed;

        void Update(TimeSpan delta);

        void Continue();

        void Pause();

        void Restart();

        void Seek(TimeSpan time);

        ITween<T> WithEasing(EasingFunction easing)
        {
            Easing = easing;
            return this;
        }
    }

    public enum PlaybackDirection
    {
        Normal,
        Reverse,
        Alternate
    }

    public abstract class TweenBase<T> : ITween<T>
    {
        public T Start { get; }
        public T End { get; }
        public TimeSpan Duration { get; }
        public EasingFunction Easing { get; set; } = Easings.Linear;

        public event Action<T>? ValueChanged;

        public event Action? Completed;

        public bool Loop { get; set; }

        public PlaybackDirection Direction
        {
            get;
            set
            {
                field = value;
                _forward = value is PlaybackDirection.Normal or PlaybackDirection.Alternate;
            }
        }

        private TimeSpan _elapsed;
        private bool _playing;
        private bool _forward = true;
        private Lazy<TaskCompletionSource> _completionSource = new();

        public Task Task => IsCompleted ? Task.CompletedTask : _completionSource.Value.Task;

        public bool IsPlaying => _playing;
        public bool IsCompleted { get; private set; }

        public TweenBase(T start, T end, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero) throw new ArgumentException("duration must be > 0", nameof(duration));
            Start = start;
            End = end;
            Duration = duration;
            _elapsed = TimeSpan.Zero;
            _playing = true;
            IsCompleted = false;
        }

        public abstract T Lerp(T a, T b, double t);

        public void Continue() => _playing = true;
        public void Pause() => _playing = false;
        public void Restart()
        {
            _completionSource = new();
            _elapsed = Direction is PlaybackDirection.Normal or PlaybackDirection.Alternate ? TimeSpan.Zero : Duration;
            IsCompleted = false;
            _playing = true;
        }

        public void Seek(TimeSpan time)
        {
            if (time < TimeSpan.Zero)
                _elapsed = TimeSpan.Zero;

            else if (time > Duration)
                _elapsed = Duration;

            else
                _elapsed = time;

            UpdateValue();
        }

        public void Update(TimeSpan delta)
        {
            if (!_playing || IsCompleted || Duration <= TimeSpan.Zero) return;

            _elapsed += _forward ? delta : -delta;

            if (Loop && Direction == PlaybackDirection.Normal)
            {
                _elapsed = EnsureInPeriod(_elapsed);
                UpdateValue();
                return;
            }

            if (_forward && _elapsed >= Duration)
            {
                _elapsed = Duration;
                UpdateValue();
                if (Direction == PlaybackDirection.Alternate)
                {
                    _forward = false;
                    _elapsed = Duration;
                }

                else if (Loop)
                {
                    _elapsed = TimeSpan.Zero;
                }

                else
                {
                    Complete();
                    return;
                }
            }

            else if (!_forward && _elapsed <= TimeSpan.Zero)
            {
                _elapsed = TimeSpan.Zero;
                UpdateValue();
                if (Direction == PlaybackDirection.Alternate)
                {
                    _forward = true;
                    _elapsed = TimeSpan.Zero;
                    if (!Loop)
                    {
                        Complete();
                        return;
                    }
                }

                else if (Direction == PlaybackDirection.Reverse)
                {
                    _elapsed = TimeSpan.Zero;
                    if (!Loop)
                    {
                        Complete();
                        return;
                    }
                }

                else if (Loop) _elapsed = Duration;

                else
                {
                    Complete();
                    return;
                }
            }

            else UpdateValue();
        }

        private TimeSpan EnsureInPeriod(TimeSpan elapsed)
        {
            var ticks = elapsed.Ticks % Duration.Ticks;
            if (ticks < 0) ticks += Duration.Ticks;
            return TimeSpan.FromTicks(ticks);
        }

        private void UpdateValue()
        {
            var t = Duration <= TimeSpan.Zero ? 1.0 : Math.Clamp(_elapsed.TotalSeconds / Duration.TotalSeconds, 0.0, 1.0);
            var eased = Easing(t);
            var value = Lerp(Start, End, eased);
            ValueChanged?.Invoke(value);
        }

        private void Complete()
        {
            UpdateValue();
            _playing = false;
            IsCompleted = true;
            if (_completionSource.IsValueCreated) _completionSource.Value.SetResult();
            Completed?.Invoke();
        }
    }

    public class Tween<T> : TweenBase<T> where T : INumber<T>
    {
        public Tween(T start, T end, TimeSpan duration) : base(start, end, duration)
        { }

        public override T Lerp(T a, T b, double t)
        {
            var at = double.CreateChecked(a);
            var bt = double.CreateChecked(b);
            return T.CreateChecked(at + (bt - at) * t);
        }
    }

    public class TweenVector2 : TweenBase<Vector2>
    {
        public TweenVector2(Vector2 start, Vector2 end, TimeSpan duration) : base(start, end, duration)
        { }

        public override Vector2 Lerp(Vector2 a, Vector2 b, double t)
        {
            var x = a.X + ((double)b.X - a.X) * t;
            var y = a.Y + ((double)b.Y - a.Y) * t;
            return new((float)x, (float)y);
        }
    }

    public class TweenColor : TweenBase<Color>
    {
        public TweenColor(Color start, Color end, TimeSpan duration) : base(start, end, duration)
        { }

        public override Color Lerp(Color a, Color b, double t)
        {
            var red = byte.CreateChecked(a.R + (b.R - a.R) * t);
            var green = byte.CreateChecked(a.G + (b.G - a.G) * t);
            var blue = byte.CreateChecked(a.B + (b.B - a.B) * t);
            var alpha = byte.CreateChecked(a.A + (b.A - a.A) * t);
            return Color.FromArgb(alpha, red, green, blue);
        }
    }
}