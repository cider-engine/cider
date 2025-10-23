using Cider.Data.In2D;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using System;
using CiderSprite = Cider.Data.In2D.Sprite;

namespace Cider.Render.In2D
{
    public class RenderContext2D : RenderContext
    {
        public Transform2D CurrentTransform2D { get; internal set; }

        public void RenderSprite(CiderSprite sprite)
        {
            SpriteBatch.Draw(sprite, CurrentTransform2D.Position, CurrentTransform2D.RotationInRadians, CurrentTransform2D.Scale);
        }

        public RenderContext2D ApplyTransform(Transform2D transform)
        {
            CurrentTransform2D = CurrentTransform2D.ApplyTransform2D(transform);
            return this;
        }
    }
}
