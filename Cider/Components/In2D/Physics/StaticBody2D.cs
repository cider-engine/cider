using Cider.Data;
using Cider.Data.In2D;
using nkast.Aether.Physics2D.Dynamics;

namespace Cider.Components.In2D.Physics
{
    public class StaticBody2D : PhysicsBody2D
    {
        public StaticBody2D()
        {
            Body.BodyType = BodyType.Static;
        }
    }
}
