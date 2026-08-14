using System;
using System.Numerics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Cider.Animation
{

    public static class Tween
    {
        public static ITween<T> Create<T>(T startValue, T endValue, Action<T> valueSetter, TimeSpan duration, PlaybackDirection direction = PlaybackDirection.Normal) where T : INumber<T>
        {
            var tween = new TweenNumber<T>(startValue, endValue, duration, direction);
            tween.ValueChanged += valueSetter;
            return tween;
        }

        public static ITween<Vector2> Create(Vector2 startValue, Vector2 endValue, Action<Vector2> valueSetter, TimeSpan duration, PlaybackDirection direction = PlaybackDirection.Normal)
        {
            var tween = new TweenVector2(startValue, endValue, duration, direction);
            tween.ValueChanged += valueSetter;
            return tween;
        }

        public static ITween<Color> Create(Color startValue, Color endValue, Action<Color> valueSetter, TimeSpan duration, PlaybackDirection direction = PlaybackDirection.Normal)
        {
            var tween = new TweenColor(startValue, endValue, duration, direction);
            tween.ValueChanged += valueSetter;
            return tween;
        }
    }

    public interface ITween<T> : IUpdatable
    {
        T StartValue { get; }

        T EndValue { get; }

        TimeSpan Duration { get; }

        EasingFunction Easing { get; set; }

        bool IsLooping { get; set; }

        PlaybackDirection Direction { get; set; }

        bool IsPlaying { get; }

        bool IsCompleted { get; }

        event Action<T> ValueChanged;

        event Action Completed;

        void Continue();

        void Pause();

        void Restart();

        void Seek(TimeSpan time);

        ITween<T> WithEasing(EasingFunction easing)
        {
            Easing = easing;
            return this;
        }

        TweenEndAwaitable<T> WaitForComplete() => new(this);
    }

    public enum PlaybackDirection
    {
        Normal,
        Reverse,
        Alternate
    }

    public abstract class TweenBase<T> : ITween<T>
    {
        public T StartValue { get; }
        public T EndValue { get; }
        public TimeSpan Duration { get; }
        public EasingFunction Easing { get; set; } = Easings.Linear;

        public event Action<T>? ValueChanged;

        public event Action? Completed;

        public bool IsLooping { get; set; }

        public PlaybackDirection Direction
        {
            get;
            set
            {
                field = value;
                _forward = value switch
                {
                    PlaybackDirection.Normal or PlaybackDirection.Alternate => true,
                    PlaybackDirection.Reverse => false,
                    _ => throw new ArgumentException(nameof(Direction))
                };
            }
        }

        private TimeSpan _elapsed;
        private bool _forward = true;

        public bool IsPlaying { get; private set; } = true;
        public bool IsCompleted { get; private set; } = false;

        public TweenBase(T start, T end, TimeSpan duration, PlaybackDirection direction)
        {
            if (duration <= TimeSpan.Zero) throw new ArgumentException("duration must be > 0", nameof(duration));
            StartValue = start;
            EndValue = end;
            Duration = duration;
            Direction = direction;
            _elapsed = direction is PlaybackDirection.Normal or PlaybackDirection.Alternate ? TimeSpan.Zero : duration;
        }

        public abstract T Lerp(T a, T b, double t);

        public void Continue() => IsPlaying = true;
        public void Pause() => IsPlaying = false;
        public void Restart()
        {
            _elapsed = Direction is PlaybackDirection.Normal or PlaybackDirection.Alternate ? TimeSpan.Zero : Duration;
            IsCompleted = false;
            IsPlaying = true;
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
            if (!IsPlaying || IsCompleted) return;

            _elapsed += _forward ? delta : -delta;

            switch ((_forward, IsLooping, Direction))
            {
                case (true, false, PlaybackDirection.Normal):
                    if (_elapsed >= Duration)
                    {
                        _elapsed = Duration;
                        Complete();
                        return;
                    }
                    UpdateValue();
                    return;

                case (true, true, PlaybackDirection.Normal):
                    _elapsed = EnsureInPeriod(_elapsed);
                    UpdateValue();
                    return;

                case (false, _, PlaybackDirection.Normal): throw new InvalidOperationException();

                case (true, _, PlaybackDirection.Reverse): throw new InvalidOperationException();

                case (false, false, PlaybackDirection.Reverse):
                    if (_elapsed <= TimeSpan.Zero)
                    {
                        _elapsed = TimeSpan.Zero;
                        Complete();
                        return;
                    }
                    UpdateValue();
                    return;

                case (false, true, PlaybackDirection.Reverse):
                    _elapsed = EnsureInPeriod(_elapsed);
                    UpdateValue();
                    return;

                case (true, _, PlaybackDirection.Alternate):
                    if (_elapsed >= Duration)
                    {
                        _elapsed = 2 * Duration - _elapsed;
                        _forward = false;
                    }
                    UpdateValue();
                    return;

                case (false, false, PlaybackDirection.Alternate):
                    if (_elapsed <= TimeSpan.Zero)
                    {
                        _elapsed = TimeSpan.Zero;
                        Complete();
                        return;
                    }
                    UpdateValue();
                    return;

                case (false, true, PlaybackDirection.Alternate):
                    if (_elapsed <= TimeSpan.Zero)
                    {
                        _elapsed = -_elapsed;
                        _forward = true;
                    }
                    UpdateValue();
                    return;
            }

            Game.Assert(false);
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
            var value = Lerp(StartValue, EndValue, eased);
            ValueChanged?.Invoke(value);
        }

        private void Complete()
        {
            UpdateValue();
            IsPlaying = false;
            IsCompleted = true;
            Completed?.Invoke();
        }
    }

    public class TweenNumber<T> : TweenBase<T> where T : INumber<T>
    {
        public TweenNumber(T start, T end, TimeSpan duration, PlaybackDirection direction) : base(start, end, duration, direction)
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
        public TweenVector2(Vector2 start, Vector2 end, TimeSpan duration, PlaybackDirection direction) : base(start, end, duration, direction)
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
        public TweenColor(Color start, Color end, TimeSpan duration, PlaybackDirection direction) : base(start, end, duration, direction)
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

    public readonly struct TweenEndAwaitable<T>(ITween<T> tween)
    {
        public TweenEndAwaiter<T> GetAwaiter() => new(tween);
    }

    public readonly struct TweenEndAwaiter<T>(ITween<T> tween) : ICriticalNotifyCompletion
    {
        public bool IsCompleted => tween.IsCompleted;

        public void GetResult()
        {
            if (!IsCompleted) throw new InvalidOperationException("Calling GetResult when IsCompleted is false is invalid");
        }

        public void OnCompleted(Action continuation)
        {
            tween.Completed += continuation;
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            tween.Completed += continuation;
        }
    }
}