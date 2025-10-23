using Cider.Data.In2D;
using Cider.Render.In2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Render
{
    public class RenderContext
    {
        public required GameTime GameTime { get; init; }

        public required SpriteBatch SpriteBatch { get; init; }
    }
}
