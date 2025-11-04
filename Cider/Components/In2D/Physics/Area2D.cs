using Cider.Attributes;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Components.In2D.Physics
{
    public class Area2D : CollisionObject2D
    {
        public Area2D()
        {
            Body.BodyType = BodyType.Static;
        }

        protected override void OnShape2DAdded(Shape2D shape)
        {
            shape.Attach(Body, true);
        }

        protected override bool OnCollision(Fixture sender, Fixture other, Contact contact)
        {
            OnBodyEntered(this, (CollisionObject2D)other.Body.Tag);
            return true;
        }

        protected override void OnSeparation(Fixture sender, Fixture other, Contact contact)
        {
            OnBodyExited(this, (CollisionObject2D)other.Body.Tag);
        }

        [Dispatcher]
        protected virtual void OnBodyEntered(Area2D sender, CollisionObject2D other)
        {
            BodyEntered?.Invoke(sender, other);
        }

        [Dispatcher]
        protected virtual void OnBodyExited(Area2D sender, CollisionObject2D other)
        {
            BodyExited?.Invoke(sender, other);
        }

        public event BodyContactDelegate BodyEntered;

        public event BodyContactDelegate BodyExited;

        public delegate void BodyContactDelegate(Area2D sender, CollisionObject2D other);
    }
}
