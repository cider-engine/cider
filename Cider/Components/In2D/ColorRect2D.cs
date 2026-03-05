using Cider.Input;
using Cider.Render;
using System;
using System.Drawing;

namespace Cider.Components.In2D
{
    public class ColorRect2D : Component2D
    {
        public Color Color { get; set; } = Color.White;

        public float Width { get; set; }

        public float Height { get; set; }

        protected override bool HitTest(HitTestResult result)
        {
            return RectangleHitTest(result, Width, Height);
        }

        protected override void OnRender(RenderContext context)
        {
            var transform = GlobalTransform;
            context.FillRectangle(transform.Position, Width, Height, transform.RotationInRadians, Color, transform.Scale);
        }
    }
}
