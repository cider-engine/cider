using Cider.Data.In2D;
using nkast.Aether.Physics2D.Dynamics;

namespace Cider.Components.In2D.Physics
{
    public class PhysicsBody2D : CollisionObject2D
    {
        public Vector2 Velocity
        {
            get => Body.LinearVelocity;
            set => Body.LinearVelocity = value;
        }
    }
}
