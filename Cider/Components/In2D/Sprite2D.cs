using Cider.Data.In2D;
using Cider.Render.In2D;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Components.In2D
{
    public class Sprite2D : Component2D
    {
        public Sprite Sprite { get; set; }

        protected override void OnDraw2D(RenderContext2D context)
        {
            context.RenderSprite(Sprite);
        }
    }
}
