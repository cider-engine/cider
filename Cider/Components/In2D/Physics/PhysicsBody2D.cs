using Cider.Extensions;
using nkast.Aether.Physics2D.Dynamics;
using System.Numerics;

namespace Cider.Components.In2D.Physics
{
    public class PhysicsBody2D : CollisionObject2D
    {
        public Vector2 Velocity
        {
            get => Body.LinearVelocity.AsVector2();
            set => Body.LinearVelocity = value.AsPhysicsVector2();
        }
    }
}
