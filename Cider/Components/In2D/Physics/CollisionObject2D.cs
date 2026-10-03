using Cider.Collections;
using Cider.Data;
using Cider.Data.In2D;
using Cider.Extensions;
using Cider.Physics;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using System;
using System.Diagnostics;

namespace Cider.Components.In2D.Physics
{
    public class CollisionObject2D : Component2D
    {
        private Transform2D _previousLocal = new();
        private Transform2D _currentLocal = new();
        private TimeSpan _previousToCurrent = TimeSpan.Zero;
        private TimeSpan _interpolationAccumulator = TimeSpan.Zero;
        private bool _changeTransformFromInterpolation = false;

        public CollisionObject2D()
        {
            Body = new()
            {
                Tag = this
            };

            Shapes.ItemAdded += (collection, shape) =>
            {
                OnShape2DAdded(shape);
            };

            Shapes.ItemRemoved += (collection, shape) =>
            {
                OnShape2DRemoved(shape);
            };
        }

        public ItemObservableCollection<Shape2D> Shapes { get; } = new();

        protected virtual void OnShape2DAdded(Shape2D shape)
        {
            shape.Attach(Body);
        }

        protected virtual void OnShape2DRemoved(Shape2D shape)
        {
            shape.Detach();
        }

        internal Body Body { get; }

        private protected override void OnAttachToSceneInternal(Scene root)
        {
            Body.OnCollision += OnCollision;
            Body.OnSeparation += OnSeparation;
            root.EnqueueBodyToAdd2D(Body);
        }

        private protected override void OnDetachFromSceneInternal(Scene root)
        {
            root.EnqueueBodyToRemove2D(Body);
            Body.OnCollision -= OnCollision;
            Body.OnSeparation -= OnSeparation;
        }

        protected virtual bool OnCollision(Fixture sender, Fixture other, Contact contact)
        {
            return true;
        }

        protected virtual void OnSeparation(Fixture sender, Fixture other, Contact contact)
        {
        }

        private protected override void OnGlobalTransformChangedInternal(EventArgs args)
        {
            // 如果是插值在修改Transform就跳过下面的物理对象同步
            if (_changeTransformFromInterpolation)
            {
                _changeTransformFromInterpolation = false;
                base.OnGlobalTransformChangedInternal(args);
                return;
            }

            // 手动修改Transform，强制同步到Body
            var globalTransform = ((Transform2DChangedEventArgs)args).CurrentTransform2D;
            var globalPosition = globalTransform.Position;
            var globalRotation = globalTransform.RotationInRadians;

            // 应该换一个更好的方式来阻止物理插值修改Transform时强制同步到物理位置
            // 但是这里不用在物体瞬移的时候多写一个重置插值的方法
            _previousLocal = _currentLocal = globalTransform;

            if (globalPosition != Body.Position.AsVector2())
            {
                Body.Position = globalPosition.AsPhysicsVector2();
            }

            if (globalRotation != Body.Rotation)
            {
                Body.Rotation = globalRotation;
            }

            base.OnGlobalTransformChangedInternal(args);
        }

        private protected override void OnUpdateInternal(TimeContext context)
        {
            if (Game.Instance.ProjectSettings.IsPhysicsInterpolationEnabled)
            {
                _interpolationAccumulator += context.DeltaTime;

                if (Transform != _currentLocal)
                {
                    if (_interpolationAccumulator >= _previousToCurrent)
                    {
                        Debug.Assert(!_changeTransformFromInterpolation);
                        _changeTransformFromInterpolation = true;
                        Transform = _currentLocal;
                    }

                    else
                    {
                        Debug.Assert(!_changeTransformFromInterpolation);
                        _changeTransformFromInterpolation = true;
                        Transform = Transform2D.Lerp(_previousLocal, _currentLocal, _interpolationAccumulator / _previousToCurrent);
                    }
                }
            }

            base.OnUpdateInternal(context);
        }

        private protected override void OnFixedUpdateInternal(TimeContext context)
        {
            _previousLocal = _currentLocal;
            _interpolationAccumulator = TimeSpan.Zero;
            _previousToCurrent = context.DeltaTime;
            if (TryGetNewLocalTransform(out var transform))
            {
                _currentLocal = transform;

                if (!Game.Instance.ProjectSettings.IsPhysicsInterpolationEnabled)
                    Transform = transform;
            }

            base.OnFixedUpdateInternal(context);
        }

        private bool TryGetNewLocalTransform(out Transform2D transform)
        {
            // 从物理世界获取期望的全局位置和旋转
            var targetGlobalPosition = Body.Position.AsVector2();
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
                transform = Transform with
                {
                    Position = newLocalTransform.Position,
                    RotationInRadians = newLocalTransform.RotationInRadians
                };

                return true;
            }

            transform = default; // 这个transform不应该使用，所以直接default而不是无参构造
            return false;
        }
    }
}
