using Cider.Assets;
using Cider.Data;
using Cider.Render.In2D;
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

        protected override void OnDraw2D(RenderContext2D context)
        {
            if (Texture is null) return;

            context.SpriteBatch.Draw(
                texture: Texture.Get(),

                position: context.CurrentTransform2D.Position,

                sourceRectangle: new(0, 0, Texture.Width, Texture.Height),

                color: Color.White,

                rotation: context.CurrentTransform2D.RotationInRadians,

                scale: context.CurrentTransform2D.Scale,

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
