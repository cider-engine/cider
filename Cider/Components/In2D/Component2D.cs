using Cider.Data.In2D;
using Cider.Render;
using Cider.Render.In2D;
using MonoGame.Extended.Input.InputListeners;
using System;

namespace Cider.Components.In2D
{
    public class Component2D : Component
    {
        public Vector2 Position
        {
            get => Transform.Position;
            set => Transform = new Transform2D(value, Transform.RotationInRadians, Transform.Scale);
        }

        public Transform2D Transform { get; set; } = new();

        public Transform2D GlobalTransform
        {
            get
            {
                var globalTransform = Transform;
                var parent = Parent;
                while (parent is Component2D parent2D)
                {
                    globalTransform = globalTransform.ApplyTransform2D(parent2D.Transform);
                    parent = parent.Parent;
                }

                return globalTransform;
            }
        }

        public event EventHandler<MouseEventArgs> MouseDown;

        public event EventHandler<MouseEventArgs> MouseUp;

        protected virtual void OnDraw2D(RenderContext2D context)
        {
        }

        protected internal override void OnDraw(RenderContext context)
        {
            if (!IsVisible) return;

            var context2D = context is RenderContext2D ctx
                ? ctx.ApplyTransform(Transform)
                : new()
                {
                    GameTime = context.GameTime,
                    SpriteBatch = context.SpriteBatch,
                    CurrentTransform2D = Transform,
                };

            var toBeRestored = context2D.CurrentTransform2D;

            OnDraw2D(context2D);

            foreach (var item in Children)
            {
                item.OnDraw(context2D);
                context2D.CurrentTransform2D = toBeRestored;
            }
        }

#nullable enable
        protected internal virtual void OnMouseDown(object? sender, MouseEventArgs args)
        {
            MouseDown?.Invoke(sender, args);
        }


        protected internal virtual void OnMouseUp(object? sender, MouseEventArgs args)
        {
            MouseUp?.Invoke(sender, args);
        }
    }
}
