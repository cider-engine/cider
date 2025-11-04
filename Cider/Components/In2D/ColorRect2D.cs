using Cider.Data;
using Cider.Input;
using Cider.Render;
using Cider.Extensions;
using System;

namespace Cider.Components.In2D
{
    public class ColorRect2D : Component2D
    {
        public Color Color { get; set; } = Color.White;

        public float Width { get; set; }

        public float Height { get; set; }

        protected internal override bool HitTest(HitTestResult result)
        {
            return RectangleHitTest(result, Width, Height);
        }

        protected override void OnRender(RenderContext context)
        {
            var transform = GlobalTransform;
            context.SpriteBatch.FillRectangle(transform.Position, Width, Height, transform.RotationInRadians, Color);
        }
    }
}
