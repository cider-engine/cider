using Cider.Data.In2D;
using Cider.Input;
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

        internal override void ForeachHitTest(HitTestResult result)
        {
            if (!IsVisible) return;

            result.ApplyTransform(Transform);
            var toBeRestored = result.CurrentTransform2D;

            if (HitTest(result)) result.SetComponent(this);

            foreach (var item in Children)
            {
                item.ForeachHitTest(result);
                result.CurrentTransform2D = toBeRestored;
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
#nullable disable

        internal static bool RectangleHitTest(HitTestResult result, float width, float height)
        {
            var transform = result.CurrentTransform2D;
            var vector = Vector2.FromPoint(result.EventArgs.Position);

            // 去平移：相对 transform 的位置
            var local = vector - transform.Position;

            // 去旋转：按 -rotation 旋转回本地方向
            local = local.Rotate(-transform.RotationInRadians);

            // 避免除以零（缩放为0视为不可点击）
            const float eps = 1e-6f;
            if (Math.Abs(transform.Scale.X) < eps || Math.Abs(transform.Scale.Y) < eps)
                return false;

            // 去缩放：得到本地坐标
            local = new Vector2(local.X / transform.Scale.X, local.Y / transform.Scale.Y);

            // 判断是否在矩形 [0, Width] x [0, Height] 内
            if (local.X >= 0f && local.Y >= 0f && local.X <= width && local.Y <= height)
            {
                return true;
            }

            return false;
        }
    }
}
