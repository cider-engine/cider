using Cider.Data;
using Cider.Data.In2D;
using nkast.Aether.Physics2D.Collision;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Components.In2D.Physics
{
    public class CollisionObject2D : Component2D
    {
        public CollisionObject2D()
        {
            Body = new()
            {
                Tag = this
            };

            Children.ComponentAdded += (owner, child) =>
            {
                if (child is Shape2D shape)
                {
                    OnShape2DAdded(shape);
                }
            };

            Children.ComponentRemoved += (owner, child) =>
            {
                if (child is Shape2D shape)
                {
                    OnShape2DRemoved(shape);
                }
            };
        }

        protected virtual void OnShape2DAdded(Shape2D shape)
        {
            shape.Attach(Body);
        }

        protected virtual void OnShape2DRemoved(Shape2D shape)
        {
            shape.Detach(Body);
        }

        internal Body Body { get; }

        protected internal override void OnAttachToScene(Scene root)
        {
            Body.OnCollision += OnCollision;
            Body.OnSeparation += OnSeparation;
            root.EnqueueBodyToAdd2D(Body);
            base.OnAttachToScene(root);
        }

        protected internal override void OnDetachToScene(Scene root)
        {
            root.EnqueueBodyToRemove2D(Body);
            Body.OnCollision -= OnCollision;
            Body.OnSeparation -= OnSeparation;
            base.OnDetachToScene(root);
        }

        protected virtual bool OnCollision(Fixture sender, Fixture other, Contact contact)
        {
            return true;
        }

        protected virtual void OnSeparation(Fixture sender, Fixture other, Contact contact)
        {
        }

        protected override void OnGlobalTransformChanged(EventArgs args)
        {
            var globalTransform = ((Transform2DChangedEventArgs)args).CurrentTransform2D;
            var globalPosition = globalTransform.Position;
            var globalRotation = globalTransform.RotationInRadians;

            if (globalPosition != (Vector2)Body.Position)
            {
                Body.Position = globalPosition;
            }

            if (globalRotation != Body.Rotation)
            {
                Body.Rotation = globalRotation;
            }
        }

        protected internal override void OnFixedUpdate(in TimeContext context)
        {
            // 从物理世界获取期望的全局位置和旋转
            var targetGlobalPosition = (Vector2)Body.Position;
            var targetGlobalRotation = Body.Rotation;

            var globalTransform = GlobalTransform;

            // 如果当前全局位置或旋转与目标不同，则更新
            if (globalTransform.Position != targetGlobalPosition || globalTransform.RotationInRadians != targetGlobalRotation)
            {
                // 创建一个包含目标位置和旋转，但保持当前全局缩放的临时全局变换
                var targetGlobalTransform = new Transform2D(targetGlobalPosition, targetGlobalRotation, globalTransform.Scale);

                // 计算父级全局变换的逆，并将其应用到目标全局变换上，以求得新的局部变换
                var newLocalTransform = _parentGlobalTransform.Invert().ApplyTransform2D(targetGlobalTransform);

                // 更新当前组件的局部位置和旋转
                Transform = Transform with
                {
                    Position = newLocalTransform.Position,
                    RotationInRadians = newLocalTransform.RotationInRadians
                };
            }

            base.OnFixedUpdate(context);
        }
    }
}
