using Cider.Data.In2D.Physics;
using nkast.Aether.Physics2D.Dynamics;
using System;

namespace Cider.Components.In2D.Physics
{
    public class StaticBody2D : Component2D
    {
        public Shape2D Shape
        {
            get;
            set
            {
                if (value is null) throw new NullReferenceException();
                if (field?.Body is not null)
                    field.Detach(Body);
                value.Attach(Body);
                field = value;
            }
        }

        public Body Body { get; } = new Body()
        {
            BodyType = BodyType.Static,
        };

        protected internal override void OnAttachToScene(Scene root)
        {
            Body.Position = GlobalTransform.Position;
            root.World2D.Add(Body);
            base.OnAttachToScene(root);
        }

        protected internal override void OnDetachToScene(Scene root)
        {
            root.World2D.Remove(Body);
            base.OnDetachToScene(root);
        }
    }
}
