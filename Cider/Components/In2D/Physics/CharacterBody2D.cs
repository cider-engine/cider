using nkast.Aether.Physics2D.Dynamics;
using System;

namespace Cider.Components.In2D.Physics
{
    public class CharacterBody2D : PhysicsBody2D
    {
        public CharacterBody2D()
        {
            Body.BodyType = BodyType.Dynamic;
            Body.FixedRotation = true;
        }

        static readonly float yThreshold = (float)Math.Cos(45 * Math.PI / 180);

        public bool IsOnFloor()
        {
            for (var contactEdge = Body.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
            {
                var contact = contactEdge.Contact;
                if (!contact.IsTouching)
                    continue;

                if (contact.FixtureA.IsSensor || contact.FixtureB.IsSensor)
                    continue;

                contact.GetWorldManifold(out var normal, out _);
                if (contact.FixtureB.Body == Body) // 目前我还没测出来啥情况下FixtureB.Body == Body
                    normal = -normal;

                if (normal.Y >= yThreshold)
                    return true;
            }
            return false;
        }
    }
}
