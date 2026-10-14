using Cider.Data.In2D;
using Cider.Input;
using Cider.Render;
using System;
using System.Numerics;

namespace Cider.Components.In2D
{
    public class Sprite2D : Component2D, ISizeMeasurable
    {
        private RectangleF? _cachedRenderRegion = null;

        public TextureContainer Texture
        {
            get;
            set
            {
                if (field != value) _cachedRenderRegion = null;
                field = value;
            }
        }

        public bool IsCentered { get; set { field = value; _cachedRenderRegion = null; } } = true;

        public FlipMode FlipMode { get; set; }

        public RectangleF? Region { get; set { field = value; _cachedRenderRegion = null; } } = null;

        public int FrameIndex
        {
            get;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 0, nameof(FrameIndex));

                field = value;

                _cachedRenderRegion = null;
            }
        }

        public int HorizontalFrameCount
        {
            get;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1, nameof(HorizontalFrameCount));

                field = value;

                _cachedRenderRegion = null;
            }
        } = 1;

        public int VerticalFrameCount
        {
            get;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1, nameof(VerticalFrameCount));

                field = value;

                _cachedRenderRegion = null;
            }
        } = 1;

        public Color Color { get; set; } = Color.White;

        private void UpdateRenderRegion(Texture texture)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(FrameIndex, HorizontalFrameCount * VerticalFrameCount);

            if (HorizontalFrameCount == 1 && VerticalFrameCount == 1)
            {
                _cachedRenderRegion = Region ?? new(0, 0, texture.Width, texture.Height);
                return;
            }

            var frameWidth = (float)texture.Width / HorizontalFrameCount;
            var frameHeight = (float)texture.Height / VerticalFrameCount;

            var column = FrameIndex % HorizontalFrameCount;
            var row = FrameIndex / HorizontalFrameCount;

            var x = frameWidth * column;
            var y = frameHeight * row;

            if (Region is RectangleF rect) _cachedRenderRegion = new RectangleF(
                rect.X + x,
                rect.Y + y,
                rect.Width,
                rect.Height);

            else _cachedRenderRegion = new RectangleF(
                x,
                y,
                frameWidth,
                frameHeight);
        }

        public bool TryMeasureSize(out Vector2 unscaledSize)
        {
            if (Texture is null || _cachedRenderRegion is null)
            {
                unscaledSize = default;
                return false;
            }

            unscaledSize = _cachedRenderRegion.Value.SizeVector;
            return true;
        }

        protected override bool HitTest(HitTestResult result)
        {
            if (Texture is null || _cachedRenderRegion is null || Color.A == 0) return false;
            var rect = _cachedRenderRegion.Value;
            if (IsCentered)
                return result.RectangleHitTest(rect.Width, rect.Height, rect.Width / 2, rect.Height / 2);

            else return result.RectangleHitTest(rect.Width, rect.Height);
        }

        protected override void OnRender(RenderContext context)
        {
            if (Texture is null || Color.A == 0) return;

            if (Texture.TryGetLoaded(context.Renderer, out var texture))
            {
                if (_cachedRenderRegion is null) UpdateRenderRegion(texture);

                var rect = _cachedRenderRegion!.Value;

                var transform = GlobalTransform;

                // Texture有可能在其他地方被复用
                using var colorScope = Color == Color.White ? default : context.PushTextureColor(texture, Color);
                using var blendScope = Color.A == byte.MaxValue ? default : context.PushTextureBlendMode(texture, BlendMode.Blend);

                context.RenderTexture(
                    texture: texture,

                    position: IsCentered ? transform.Position - transform.Scale * new Vector2(rect.Width / 2, rect.Height / 2) : transform.Position,

                    sourceRectangle: rect,

                    rotationInDegrees: transform.RotationInDegrees,

                    scale: transform.Scale,

                    originInSource: IsCentered ? new Vector2(rect.Width / 2, rect.Height / 2) : Vector2.Zero,

                    flipMode: FlipMode);
            }
        }
    }
}
