using Cider.Assets;
using Cider.Data;
using Cider.Data.In2D;
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
            set => field = value < TimeSpan.Zero ? throw new ArgumentOutOfRangeException(nameof(FramePerTimeSpan)) : value;
        } = TimeSpan.FromSeconds(1);

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
                    var index = int.CreateChecked(_accumulator.Ticks / FramePerTimeSpan.Ticks);

                    Debug.WriteLine(index);

                    var frame = animation.SpriteFrames[index];

                    if (frame.Texture.LoadTextureAsync(context.Renderer) is { IsCompletedSuccessfully: true } task)
                    {
                        var texture = task.Result;

                        var transform = GlobalTransform;

                        context.RenderTexture(texture, transform.Position, frame.Region, transform.RotationInDegrees, transform.Scale, Vector2.Zero, FlipMode.None);
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
            if (CurrentAnimation != null)
            {
                if (animations.TryGetValue(CurrentAnimation, out var animation))
                {
                    switch (animation.LoopMode)
                    {
                        case LoopMode.None:
                            {
                                Debug.Assert(_accumulator >= TimeSpan.Zero);
                                var lastFrame = (animation.SpriteFrames.Count - 1) * FramePerTimeSpan;
                                if (_accumulator < lastFrame)
                                    _accumulator += context.DeltaTime;
                                break;
                            }

                        case LoopMode.Linear:
                            {
                                _accumulator += context.DeltaTime;
                                var total = animation.SpriteFrames.Count * FramePerTimeSpan;
                                if (_accumulator >= total) _accumulator -= total;
                                break;
                            }

                        case LoopMode.Pingpong:
                            {
                                // TODO
                                break;
                            }

                        default:
                            throw new InvalidOperationException();
                    }
                }
            }
            base.OnUpdateInternal(context);
        }

        public void Play(string animation)
        {
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
        public required TextureAsset Texture { get; init => ArgumentNullException.ThrowIfNull(field = value); }
        public RectangleF? Region { get; init; }
    }
}
