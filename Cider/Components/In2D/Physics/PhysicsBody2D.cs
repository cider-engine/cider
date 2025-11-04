using Cider.Data;
using Cider.Data.In2D;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Text;

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
