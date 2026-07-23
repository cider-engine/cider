using Cider.Components.In2D;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Numerics;

namespace Cider.Physics
{

    public abstract class Shape2D
    {
        public Vector2 Position { get; set; }
#nullable enable
        public abstract Body? Body { get; }
#nullable disable
        public abstract void Attach(Body body, bool isSensor = false);
        public abstract void Detach(Body body);
    }
}
