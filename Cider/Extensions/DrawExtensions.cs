using Cider.Data;
using Cider.Data.In2D;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;

namespace Cider.Extensions
{
    public static class DrawExtensions
    {
        extension(SpriteBatch spriteBatch)
        {
            public void FillRectangle(Vector2 position, float width, float height, float rotation, Color color)
            {
                spriteBatch.Draw(GetTexture(null, spriteBatch),
                    position,
                    null,
                    color,
                    rotation,
                    Vector2.Zero,
                    new Vector2(width, height),
                    SpriteEffects.None, 0);

                [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = nameof(GetTexture))]
                static extern Texture2D GetTexture([UnsafeAccessorType("MonoGame.Extended.ShapeExtensions, MonoGame.Extended")] object __owner, SpriteBatch spriteBatch);
            }
        }
    }
}
