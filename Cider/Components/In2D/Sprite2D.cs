using Cider.Assets;
using Cider.Data.In2D;
using Cider.Extensions;
using Cider.Input;
using Cider.Internals;
using Cider.Render;
using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace Cider.Components.In2D
{
    public class Sprite2D : Component2D
    {
#nullable enable
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
#nullable disable

        public bool IsCentered { get; set { field = value; _cachedRenderRegion = null; } } = false;

        public bool FlipHorizontally { get; set; } = false;

        public bool FlipVertically { get; set; } = false;

        public bool RegionEnabled { get; set { field = value; _cachedRenderRegion = null; } } = false;

        public RectangleF RegionRectangle { get; set { field = value; _cachedRenderRegion = null; } } = RectangleF.Empty;

        public int FrameIndex
        {
            get;
            set
            {
                if (FrameIndex < 0 || FrameIndex >= HorizontalFrameCount * VerticalFrameCount)
                    throw new ArgumentOutOfRangeException(nameof(FrameIndex));

                field = value;

                _cachedRenderRegion = null;
            }
        }

        public int HorizontalFrameCount
        {
            get;
            set
            {
                if (FrameIndex >= VerticalFrameCount * value)
                    throw new ArgumentOutOfRangeException(nameof(HorizontalFrameCount));

                field = value;

                _cachedRenderRegion = null;
            }
        } = 1;

        public int VerticalFrameCount
        {
            get;
            set
            {
                if (FrameIndex >= HorizontalFrameCount * value)
                    throw new ArgumentOutOfRangeException(nameof(VerticalFrameCount));

                field = value;

                _cachedRenderRegion = null;
            }
        } = 1;

        private void UpdateRenderRegion(Renderer renderer)
        {
            if (Texture is null) return;

            if (Texture?.LoadTexture(renderer) is Task<Texture> { IsCompletedSuccessfully: true } task)
            {
                var texture = task.Result;

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
        }

        protected override bool HitTest(HitTestResult result)
        {
            if (Texture is null || _cachedRenderRegion is null) return false;
            var rect = _cachedRenderRegion.Value;
            if (IsCentered)
                return RectangleHitTest(result, rect.Width, rect.Height, rect.Width / 2f, rect.Height / 2f);

            else return RectangleHitTest(result, rect.Width, rect.Height);
        }

        protected override void OnRender(RenderContext context)
        {
            if (Texture is null) return;

            if (Texture?.LoadTexture(context.Renderer) is Task<Texture> { IsCompletedSuccessfully: true } task)
            {
                if (_cachedRenderRegion is RectangleF rect)
                {
                    var transform = GlobalTransform;

                    context.RenderTexture(
                        texture: task.Result,

                        position: IsCentered ? transform.Position - new Vector2(rect.Width / 2f, rect.Height / 2f) : transform.Position,

                        sourceRectangle: rect,

                        rotationInDegrees: transform.RotationInDegrees,

                        scale: transform.Scale,

                        origin: IsCentered ? new(rect.Width / 2f, rect.Height / 2f) : Vector2.Zero,

                        flipMode: (FlipHorizontally
                            ? FlipMode.FlipHorizontally
                            : FlipMode.None) |
                        (FlipVertically
                            ? FlipMode.FlipVertically
                            : FlipMode.None));
                }

                else
                    UpdateRenderRegion(context.Renderer);
            }
        }
    }
}
