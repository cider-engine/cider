using Cider.Data;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
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
            ContactEdge contactEdge = Body.ContactList;
            while (contactEdge != null)
            {
                Contact contact = contactEdge.Contact;

                if (contact.IsTouching)
                {
                    contact.GetWorldManifold(out var normal, out _);

                    if (contact.FixtureA.Body != Body && contact.FixtureB.Body != Body)
                    {
                        contactEdge = contactEdge.Next;
                        continue;
                    }

                    if (normal.Y >= yThreshold)
                        return true;
                }

                contactEdge = contactEdge.Next;
            }
            return false;
        }
    }
}
