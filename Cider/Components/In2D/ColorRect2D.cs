using Cider.Data.In2D;
using Cider.Input;
using Cider.Render;
using System;
using System.Numerics;

namespace Cider.Components.In2D
{
    public class ColorRect2D : Component2D, ISizeMeasurable
    {
        public Color Color { get; set; } = Color.White;

        public float Width { get; set => ArgumentOutOfRangeException.ThrowIfNegative(field = value); }

        public float Height { get; set => ArgumentOutOfRangeException.ThrowIfNegative(field = value); }

        public bool TryMeasureSize(out Vector2 unscaledSize)
        {
            unscaledSize = new(Width, Height);
            return true;
        }

        protected override bool HitTest(HitTestResult result)
        {
            return result.RectangleHitTest(Width, Height);
        }

        protected override void OnRender(RenderContext context)
        {
            var transform = GlobalTransform;
            context.RenderFilledRect(transform.Position, Width, Height, transform.RotationInRadians, Color, transform.Scale);
        }
    }
}
