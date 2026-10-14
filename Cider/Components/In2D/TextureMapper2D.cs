using Cider.Render;
using System;

namespace Cider.Components.In2D
{
    public class TextureMapper2D : Component2D
    {
        public TextureContainer Texture { get; set; }

        public TextureMappingVertex? TopLeft { get; set; }

        public TextureMappingVertex? TopRight { get; set; }

        public TextureMappingVertex? BottomLeft { get; set; }

        public TextureMappingVertex? BottomRight { get; set; }

        public TextureMapper2D()
        {
            Children.ComponentAdded += ChildrenComponentAdded;
        }

        private static void ChildrenComponentAdded(Component owner, Component changedComponent)
        {
            throw new InvalidOperationException();
        }

        protected override void OnRender(RenderContext context)
        {
            if (Texture.TryGetLoaded(context.Renderer, out var texture))
            {
                var topLeft = TopLeft ?? throw new ArgumentNullException(nameof(TopLeft));
                var topRight = TopRight ?? throw new ArgumentNullException(nameof(TopRight));
                var bottomLeft = BottomLeft ?? throw new ArgumentNullException(nameof(BottomLeft));
                var bottomRight = BottomRight ?? throw new ArgumentNullException(nameof(BottomRight));

                if (context.Renderer.Camera2D is { IsEnabled: true, OffsetPosition: var offset })
                {
                    topLeft.Position -= offset;
                    topRight.Position -= offset;
                    bottomLeft.Position -= offset;
                    bottomRight.Position -= offset;
                }

                context.RenderGeometry([topLeft, topRight, bottomLeft, bottomRight], texture, [0, 1, 2, 2, 1, 3]);
            }
        }
    }
}
