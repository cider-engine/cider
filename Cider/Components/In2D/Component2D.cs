using Cider.Attributes;
using Cider.Data.In2D;
using Cider.Extensions;
using Cider.Input;
using System;
using System.Numerics;

namespace Cider.Components.In2D
{
#nullable enable
    public delegate void GotFocusEventHandler(Component2D sender, Component2D? lostFocusComponent);
    public delegate void LostFocusEventHandler(Component2D sender, Component2D? gotFocusComponent);
#nullable restore

    public class Component2D : Component
    {
        public Vector2 Position
        {
            get => Transform.Position;
            set => Transform = new Transform2D(value, Transform.RotationInRadians, Transform.Scale);
        }

        public Vector2 Scale
        {
            get => Transform.Scale;
            set => Transform = new Transform2D(Transform.Position, Transform.RotationInRadians, value);
        }

        public float RotationInRadians
        {
            get => Transform.RotationInRadians;
            set => Transform = new Transform2D(Transform.Position, value, Transform.Scale);
        }

        public float RotationInDegrees
        {
            get => RotationInRadians * (180 / MathF.PI);
            set => RotationInRadians = value * (MathF.PI / 180);
        }

        public Transform2D Transform
        {
            get;
            set
            {
                field = value;
                var args = new Transform2DChangedEventArgs()
                {
                    CurrentTransform2D = GlobalTransform
                };
                OnGlobalTransformChangedInternal(args);
                var toBeRestored = args.CurrentTransform2D;
                foreach (var item in Children)
                {
                    item.OnGlobalTransformChangedDispatcher(args);
                    args.CurrentTransform2D = toBeRestored;
                }
            }
        } = new();

        private protected Transform2D _parentGlobalTransform = new();

        public Transform2D GlobalTransform
        {
            get => _parentGlobalTransform.ApplyTransform2D(Transform);
        }

        public bool IsMouseOver { get; internal set; }

        public bool IsFocused { get; internal set; }

        public bool Focusable { get; private set; }

        public event EventHandler<Component, MouseButtonEventArgs> MouseDown;

        public event EventHandler<Component, MouseButtonEventArgs> MouseUp;

        public event EventHandler<Component, MouseMovedEventArgs> MouseMoved;

        public event EventHandler<Component, MouseMovedEventArgs> MouseEnter;

        public event EventHandler<Component, MouseMovedEventArgs> MouseLeave;

        public event GotFocusEventHandler GotFocus;

        public event LostFocusEventHandler LostFocus;

        private protected override EventArgs CreateGlobalTransformArgsFromCurrent() => new Transform2DChangedEventArgs()
        {
            CurrentTransform2D = GlobalTransform
        };

        internal override void OnGlobalTransformChangedDispatcher(EventArgs args)
        {
            if (args is Transform2DChangedEventArgs args2D)
            {
                _parentGlobalTransform = args2D.CurrentTransform2D;
                args2D.ApplyTransform(Transform);
                OnGlobalTransformChangedInternal(args2D);

                var toBeRestored = args2D.CurrentTransform2D;
                foreach (var item in Children)
                {
                    item.OnGlobalTransformChangedDispatcher(args2D);
                    args2D.CurrentTransform2D = toBeRestored;
                }
            }

            else
            {
                _parentGlobalTransform = new();
                var newArgs2D = new Transform2DChangedEventArgs()
                {
                    CurrentTransform2D = Transform
                };
                OnGlobalTransformChangedInternal(newArgs2D);

                var toBeRestored = newArgs2D.CurrentTransform2D;
                foreach (var item in Children)
                {
                    item.OnGlobalTransformChangedDispatcher(newArgs2D);
                    newArgs2D.CurrentTransform2D = toBeRestored;
                }
            }
        }

        [Dispatcher]
        internal override void HitTestDispatcher(HitTestResult result)
        {
            if (!IsVisible) return;

            result.ApplyTransform(Transform);
            var toBeRestored = result.CurrentTransform2D;

            if (HitTest(result)) result.SetComponent(this);

            foreach (var item in Children)
            {
                item.HitTestDispatcher(result);
                result.CurrentTransform2D = toBeRestored;
            }
        }

        private protected override void OnDetachFromSceneInternal(Scene root)
        {
            if (IsFocused) InputManager.ClearFocus();
            base.OnDetachFromSceneInternal(root);
        }

#nullable enable
        protected internal virtual void OnMouseDown(Component sender, MouseButtonEventArgs args)
        {
            MouseDown?.Invoke(sender, args);
        }

        protected internal virtual void OnMouseUp(Component sender, MouseButtonEventArgs args)
        {
            MouseUp?.Invoke(sender, args);
        }

        protected internal virtual void OnMouseMoved(Component sender, MouseMovedEventArgs args)
        {
            MouseMoved?.Invoke(sender, args);
        }

        protected internal virtual void OnMouseEnter(Component sender, MouseMovedEventArgs args)
        {
            MouseEnter?.Invoke(sender, args);
        }

        protected internal virtual void OnMouseLeave(Component sender, MouseMovedEventArgs args)
        {
            MouseLeave?.Invoke(sender, args);
        }

        protected internal virtual void OnGotFocus(Component2D sender, Component2D? lostFocusComponent)
        {
            GotFocus?.Invoke(sender, lostFocusComponent);
        }

        protected internal virtual void OnLostFocus(Component2D sender, Component2D? gotFocusComponent)
        {
            LostFocus?.Invoke(sender, gotFocusComponent);
        }
    }
}
