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
        private Task<Texture>? _underlyingTexture = null;

        public TextureAsset? Texture
        {
            get;
            set
            {
                _underlyingTexture = null;
                field = value;
                if (value is null) _cachedRenderRegion = null;
                else if (CurrentWindow is Window window)
                {
                    _underlyingTexture = value.LoadTexture(window.Renderer).EnsureToBeSuccessful();
                }
            }
        }
#nullable disable

        public bool IsCentered { get; set { field = value; UpdateRenderRegion(); } } = true;

        public bool FlipHorizontally { get; set; } = false;

        public bool FlipVertically { get; set; } = false;

        public bool RegionEnabled { get; set { field = value; UpdateRenderRegion(); } } = false;

        public RectangleF RegionRectangle { get; set { field = value; UpdateRenderRegion(); } } = new();

        public int FrameIndex
        {
            get;
            set
            {
                if (FrameIndex < 0 || FrameIndex >= HorizontalFrameCount * VerticalFrameCount)
                    throw new ArgumentOutOfRangeException(nameof(FrameIndex));

                field = value;
                UpdateRenderRegion();
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
                UpdateRenderRegion();
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
                UpdateRenderRegion();
            }
        } = 1;

        protected override void OnWindowChanged(Window oldWindow, Window newWindow)
        {
            if (Texture is null) return;
            if (oldWindow is not null)
            {
                DisposableHelpers.DisposeAndSetNull(ref _underlyingTexture);
                Texture.UnloadTexture(oldWindow.Renderer);
            }
            if (newWindow is not null)
            {
                _underlyingTexture = Texture.LoadTexture(newWindow.Renderer);
                _underlyingTexture.ContinueWith(x =>
                {
                    x.EnsureSuccess();
                    UpdateRenderRegion();
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void UpdateRenderRegion()
        {
            if (CurrentWindow is null || Texture is null || _underlyingTexture?.IsCompletedSuccessfully != true) return;

            var texture = _underlyingTexture.Result;

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
            if (Texture is null || _cachedRenderRegion is null) return false;
            var rect = _cachedRenderRegion.Value;
            if (IsCentered)
                return RectangleHitTest(result, rect.Width, rect.Height, rect.Width / 2f, rect.Height / 2f);

            else return RectangleHitTest(result, rect.Width, rect.Height);
        }

        protected override void OnRender(RenderContext context)
        {
            if (Texture is null || _underlyingTexture?.IsCompletedSuccessfully != true) return;

            var transform = GlobalTransform;
            var rect = _cachedRenderRegion.Value;

            context.RenderTexture(
                texture: _underlyingTexture.Result,

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
    }
}
