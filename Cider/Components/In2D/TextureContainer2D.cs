using Cider.Data.In2D;
using Cider.Render;
using System;
using System.Numerics;

namespace Cider.Components.In2D
{
    public class TextureContainer2D : Component2D
    {
        public Texture? Texture { get; set; } = null;

        public RectangleF? Region { get; set; } = null;

        public Vector2 Origin { get; set; } = Vector2.Zero;

        public FlipMode FlipMode { get; set; } = FlipMode.None;

        protected override void OnRender(RenderContext context)
        {
            if (Texture is null) return;

            if (Texture.OwnerRenderer != context.Renderer) throw new InvalidOperationException();

            var transform = GlobalTransform;

            context.RenderTexture(Texture, transform.Position, Region, transform.RotationInDegrees, transform.Scale, Origin, FlipMode);
        }
    }
}
