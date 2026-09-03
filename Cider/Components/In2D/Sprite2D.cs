using Cider.Assets;
using Cider.Data.In2D;
using Cider.Input;
using Cider.Render;
using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace Cider.Components.In2D
{
    public class Sprite2D : Component2D
    {
        private RectangleF? _cachedRenderRegion = null;

        public TextureAsset? Texture
        {
            get;
            set
            {
                if (field != value) _cachedRenderRegion = null;
                field = value;
            }
        }

        public bool IsCentered { get; set { field = value; _cachedRenderRegion = null; } } = true;

        public bool FlipHorizontally { get; set; } = false;

        public bool FlipVertically { get; set; } = false;

        public bool RegionEnabled { get; set { field = value; _cachedRenderRegion = null; } } = false;

        public RectangleF RegionRectangle { get; set { field = value; _cachedRenderRegion = null; } } = RectangleF.Empty;

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
                _cachedRenderRegion = RegionEnabled ? RegionRectangle : new(0, 0, texture.Width, texture.Height);
                return;
            }

            var frameWidth = (float)texture.Width / HorizontalFrameCount;
            var frameHeight = (float)texture.Height / VerticalFrameCount;

            var column = FrameIndex % HorizontalFrameCount;
            var row = FrameIndex / HorizontalFrameCount;

            var x = frameWidth * column;
            var y = frameHeight * row;

            if (RegionEnabled) _cachedRenderRegion = new RectangleF(
                RegionRectangle.X + x,
                RegionRectangle.Y + y,
                RegionRectangle.Width,
                RegionRectangle.Height);

            else _cachedRenderRegion = new RectangleF(
                x,
                y,
                frameWidth,
                frameHeight);
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
            if (Texture is null) return;

            if (Texture?.LoadTextureAsync(context.Renderer) is Task<Texture> { IsCompletedSuccessfully: true } task)
            {
                var texture = task.Result;
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

                    flipMode: (FlipHorizontally
                        ? FlipMode.FlipHorizontally
                        : FlipMode.None) |
                    (FlipVertically
                        ? FlipMode.FlipVertically
                        : FlipMode.None));
            }
        }
    }
}
