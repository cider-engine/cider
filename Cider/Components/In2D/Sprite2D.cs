using Cider.Assets;
using Cider.Data;
using Cider.Data.In2D;
using Cider.Input;
using Cider.Render;
using System;

namespace Cider.Components.In2D
{
    public class Sprite2D : Component2D
    {
        // Texture不为null时此值一定不为null
        private Rectangle? _cachedRenderRegion = null;
#nullable enable
        public Texture2DAsset? Texture
        {
            get;
            set
            {
                field = value;
                UpdateRenderRegion();
            }
        }
#nullable disable

        public bool IsCentered { get; set { field = value; UpdateRenderRegion(); } } = true;

        public bool FlipHorizontally { get; set; } = false;

        public bool FlipVertically { get; set; } = false;

        public bool RegionEnabled { get; set { field = value; UpdateRenderRegion(); } } = false;

        public Rectangle RegionRectangle { get; set { field = value; UpdateRenderRegion(); } } = new Rectangle();

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
                    throw new ArgumentOutOfRangeException();

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
                    throw new ArgumentOutOfRangeException();

                field = value;
                UpdateRenderRegion();
            }
        } = 1;

        protected internal override void OnLoaded(Scene root)
        {
            UpdateRenderRegion();
            base.OnLoaded(root);
        }

        private void UpdateRenderRegion()
        {
            if (!CiderGame.IsInitialized) return;
            if (Texture is null)
            {
                _cachedRenderRegion = null;
                return;
            }

            if (HorizontalFrameCount == 1 && VerticalFrameCount == 1)
            {
                _cachedRenderRegion = RegionEnabled ? RegionRectangle : new(0, 0, Texture.Width, Texture.Height);
                return;
            }

            var frameWidth = (float)Texture.Width / HorizontalFrameCount;
            var frameHeight = (float)Texture.Height / VerticalFrameCount;

            var column = FrameIndex % HorizontalFrameCount;
            var row = FrameIndex / HorizontalFrameCount;

            var x = frameWidth * column;
            var y = frameHeight * row;

            if (RegionEnabled) _cachedRenderRegion = new Rectangle(
                (int)(RegionRectangle.X + x),
                (int)(RegionRectangle.Y + y),
                RegionRectangle.Width,
                RegionRectangle.Height);

            else _cachedRenderRegion = new Rectangle(
                (int)x,
                (int)y,
                (int)frameWidth,
                (int)frameHeight);
        }

        protected internal override bool HitTest(HitTestResult result)
        {
            if (Texture is null || !_cachedRenderRegion.HasValue) return false;
            var rect = _cachedRenderRegion.Value;
            if (IsCentered)
                return RectangleHitTest(result, rect.Width, rect.Height, rect.Width / 2f, rect.Height / 2f);

            else return RectangleHitTest(result, rect.Width, rect.Height);
        }

        protected override void OnRender(RenderContext context)
        {
            if (Texture is null) return;

            var transform = GlobalTransform;

            var rect = _cachedRenderRegion.Value;

            context.SpriteBatch.Draw(
                texture: Texture.Get(),

                position: transform.Position,

                sourceRectangle: _cachedRenderRegion,

                color: Color.White,

                rotation: transform.RotationInRadians,

                scale: transform.Scale,

                origin: IsCentered ? new(rect.Width / 2f, rect.Height / 2f) : Microsoft.Xna.Framework.Vector2.Zero,

                effects: (FlipHorizontally
                    ? Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally
                    : Microsoft.Xna.Framework.Graphics.SpriteEffects.None) |
                (FlipVertically
                    ? Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically
                    : Microsoft.Xna.Framework.Graphics.SpriteEffects.None),

                layerDepth: 0);
        }
    }
}
