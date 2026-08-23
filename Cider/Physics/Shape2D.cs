using Cider.Components.In2D;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Numerics;

namespace Cider.Physics
{

    public abstract class Shape2D
    {
        public float Friction { get; set; } = 0.2f;
        public Vector2 Position { get; set; }

        public abstract void Attach(Body body, bool isSensor = false);
        public abstract void Detach();
    }
}
