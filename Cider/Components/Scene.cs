using Cider.Audio;
using Cider.Render;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Components
{
    public class Scene : Component
    {
        internal nkast.Aether.Physics2D.Dynamics.World World2D { get; } = new();
    }
}
