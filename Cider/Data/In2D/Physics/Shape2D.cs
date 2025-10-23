using nkast.Aether.Physics2D.Dynamics;
using System;

namespace Cider.Data.In2D.Physics
{
    public abstract class Shape2D
    {
#nullable enable
        public abstract Body? Body { get; }
#nullable disable
        public abstract void Attach(Body body);
        public abstract void Detach(Body body);
    }
}
