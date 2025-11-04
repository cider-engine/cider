using Cider.Assets;
using Cider.Data;
using Cider.Input;
using Cider.Render;
using System;

namespace Cider.Components.In2D
{
    public class Sprite2D : Component2D
    {
#nullable enable
        public Texture2DAsset? Texture { get; set; }
#nullable disable

        public bool IsCentered { get; set; } = true;

        public bool FlipHorizontally { get; set; } = false;

        public bool FlipVertically { get; set; } = false;

        protected internal override bool HitTest(HitTestResult result)
        {
            if (Texture is null) return false;
            if (IsCentered)
                return RectangleHitTest(result, Texture.Width, Texture.Height, Texture.Width / 2f, Texture.Height / 2f);

            else return RectangleHitTest(result, Texture.Width, Texture.Height);
        }

        protected override void OnRender(RenderContext context)
        {
            if (Texture is null) return;

            var transform = GlobalTransform;

            context.SpriteBatch.Draw(
                texture: Texture.Get(),

                position: transform.Position,

                sourceRectangle: new(0, 0, Texture.Width, Texture.Height),

                color: Color.White,

                rotation: transform.RotationInRadians,

                scale: transform.Scale,

                origin: IsCentered ? new(Texture.Width / 2f, Texture.Height / 2f) : Microsoft.Xna.Framework.Vector2.Zero,

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
