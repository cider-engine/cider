using Cider.Assets;
using Cider.Data;
using Cider.Data.In2D;
using Cider.Extensions;
using Cider.Render;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;

namespace Cider.Components.In2D
{
    public class AnimatedSprite2D : Component2D
    {
        private string? _currentAnimation;

        private TimeSpan _accumulator = TimeSpan.Zero;

        private readonly Dictionary<string, SpriteAnimation> animations = new();

        public string? CurrentAnimation
        {
            get => _currentAnimation;
            init => _currentAnimation = value;
        }

        public AnimationCollection SpriteAnimations { get; }

        public TimeSpan FramePerTimeSpan
        {
            get;
            set
            {
                field = value <= TimeSpan.Zero ? throw new ArgumentOutOfRangeException(nameof(FramePerTimeSpan)) : value;
                _accumulator = TimeSpan.Zero;
            }
        } = TimeSpan.FromSeconds(1);

        public FlipMode FlipMode { get; set; }

        public int FrameIndex { get; private set => ArgumentOutOfRangeException.ThrowIfNegative(field = value, nameof(FrameIndex));}

        public bool IsCentered { get; set; } = true;

        public AnimatedSprite2D()
        {
            SpriteAnimations = new AnimationCollection(animations);
            SpriteAnimations.AnimationAdded += OnAnimationAdded;
            SpriteAnimations.AnimationRemoved += OnAnimationRemoved;
        }

        private void OnAnimationAdded(AnimationCollection sender, SpriteAnimation e)
        {
        }

        private void OnAnimationRemoved(AnimationCollection sender, SpriteAnimation e)
        {
        }

        protected override void OnRender(RenderContext context)
        {
            if (CurrentAnimation is not null)
            {
                if (animations.TryGetValue(CurrentAnimation, out var animation))
                {
                    if (animation.SpriteFrames.Count == 0) return;

                    var frame = animation.SpriteFrames[FrameIndex];

                    if (frame.Texture.TryGetLoaded(context.Renderer, out var texture))
                    {
                        var transform = GlobalTransform;

                        var center = frame.Region is RectangleF rect ? new Vector2(rect.Width / 2, rect.Height / 2) : new Vector2(texture.Width / 2f, texture.Height / 2f);

                        context.RenderTexture(texture,
                            IsCentered ? (transform.Position - transform.Scale * center) : transform.Position,
                            frame.Region,
                            transform.RotationInDegrees,
                            transform.Scale,
                            IsCentered ? center : Vector2.Zero,
                            FlipMode);
                    }
                }

                else
                {
                    Game.Warning($"Animation: {CurrentAnimation} could not be found");
                }
            }
        }

        private protected override void OnUpdateInternal(TimeContext context)
        {
            if (CurrentAnimation is not null)
            {
                if (animations.TryGetValue(CurrentAnimation, out var animation))
                {
                    int frame; // 没有初始化变量使用的话编译器会自动报错

                    if (animation.SpriteFrames.Count <= 1)
                    {
                        frame = 0;
                    }

                    else switch (animation.LoopMode)
                    {
                        case LoopMode.None:
                            {
                                Debug.Assert(_accumulator >= TimeSpan.Zero);

                                var max = (animation.SpriteFrames.Count - 1) * FramePerTimeSpan;
                                if (_accumulator < max)
                                    _accumulator += context.DeltaTime;

                                frame = int.CreateChecked(_accumulator.Ticks / FramePerTimeSpan.Ticks);

                                break;
                            }

                        case LoopMode.Linear:
                            {
                                Debug.Assert(_accumulator >= TimeSpan.Zero);

                                _accumulator += context.DeltaTime;
                                var max = animation.SpriteFrames.Count * FramePerTimeSpan;
                                while (_accumulator >= max) _accumulator -= max;

                                frame = int.CreateChecked(_accumulator.Ticks / FramePerTimeSpan.Ticks);

                                break;
                            }

                        case LoopMode.Pingpong:
                            {
                                _accumulator += context.DeltaTime; // 会无限膨胀，但是TimeSpan.MaxValue太大了不用管

                                frame = int.CreateChecked(long.PingPong(_accumulator.Ticks / FramePerTimeSpan.Ticks, animation.SpriteFrames.Count - 1));

                                break;
                            }

                        default:
                            throw new InvalidOperationException();
                    }

                    
                    if (FrameIndex != frame) FrameIndex = frame;
                }
            }
            base.OnUpdateInternal(context);
        }

        public void Play(string animation)
        {
            if (_currentAnimation == animation) return;
            _currentAnimation = animation;
            _accumulator = TimeSpan.Zero;
        }
    }

    public enum LoopMode
    {
        None,
        Linear,
        Pingpong
    }

    public class SpriteAnimation : IEquatable<SpriteAnimation>
    {
        private string animation = default!;
        public required string Animation
        {
            get => animation;
            init => animation = value ?? throw new NullReferenceException();
        }

        public LoopMode LoopMode { get; set; } = LoopMode.None;

        public IList<SpriteFrame> SpriteFrames { get; } = new List<SpriteFrame>();

        public bool Equals(SpriteAnimation? other) => animation == other?.animation;

        public override bool Equals(object? obj) => Equals(obj as SpriteAnimation);

        public override int GetHashCode() => animation.GetHashCode();

        public static bool operator ==(SpriteAnimation? left, SpriteAnimation? right) => left is null ? right is null : left.Equals(right);

        public static bool operator !=(SpriteAnimation? left, SpriteAnimation? right) => !(left == right);
    }

    public readonly record struct SpriteFrame
    {
        public required TextureContainer Texture { get; init => field = value is null ? throw new ArgumentNullException(nameof(Texture)) : value; }
        public RectangleF? Region { get; init; }
    }
}
