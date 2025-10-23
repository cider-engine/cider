using Cider.Input;
using Cider.Render.In2D;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Input.InputListeners;
using System;
using System.Diagnostics;

namespace Cider.Components.In2D
{
    public class ColorRect2D : Component2D
    {
        public Color Color { get; set; } = Color.White;

        public float Width { get; set; }

        public float Height { get; set; }

        protected internal override bool HitTest(HitTestResult result)
        {
            var (x, y) = result.EventArgs.Position;
            var (globalX, globalY) = GlobalTransform.Position;
            return x >= globalX && x <= globalX + Width && y >= globalY && y <= globalY + Height;
        }

        protected override void OnDraw2D(RenderContext2D context)
        {
            var (x, y) = context.CurrentTransform2D.Position;
            context.SpriteBatch.FillRectangle(x, y, Width, Height, Color);
        }
    }
}
