using nkast.Aether.Physics2D.Dynamics;
using System;

namespace Cider.Components.In2D.Physics
{
    public abstract class Shape2D : Component2D
    {
#nullable enable
        public abstract Body? Body { get; }
#nullable disable
        public abstract void Attach(Body body, bool isSensor = false);
        public abstract void Detach(Body body);
    }
}
