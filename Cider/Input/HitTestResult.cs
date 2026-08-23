using Cider.Components;
using Cider.Data.In2D;
using Cider.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Cider.Input
{
    public class HitTestResult : IDisposable
    {
        private static readonly HitTestResult singleton = new();

        public static HitTestResult GetScopedSingleton(Vector2 position, Vector2 offset)
        {
            singleton.Position = position;
            singleton.OffsetPosition = offset;
            return singleton;
        }

        private Component? _component;

        public Transform2D CurrentTransform2D { get; set; } = new();

        public Vector2 Position { get; set; }

        public Vector2 OffsetPosition { get; set; }

        public void SetComponent([NotNull] Component control) => _component = control ?? throw new NullReferenceException();

        public Component? GetComponent() => _component;

        public HitTestResult ApplyTransform(Transform2D transform)
        {
            CurrentTransform2D = CurrentTransform2D.ApplyTransform2D(transform);
            return this;
        }

        void IDisposable.Dispose()
        {
            _component = null;
            CurrentTransform2D = new();
            Position = default;
            OffsetPosition = default;
        }

        public bool TryToLocal(out Vector2 localPosition)
        {
            var transform = CurrentTransform2D;
            var vector = Position;

            // 去平移：相对 transform 的位置
            var local = vector - transform.Position;

            // 去旋转：按 -rotation 旋转回本地方向
            local = local.Rotate(-transform.RotationInRadians);

            // 避免除以零（缩放为0视为不可点击）
            const float eps = 1e-6f;
            if (Math.Abs(transform.Scale.X) < eps || Math.Abs(transform.Scale.Y) < eps)
            {
                localPosition = Vector2.Zero;
                return false;
            }

            // 去缩放：得到本地坐标
            local = new Vector2(local.X / transform.Scale.X, local.Y / transform.Scale.Y);

            localPosition = local;

            return true;
        }

        public static bool RectangleHitTest(Vector2 localPosition, float unscaledWidth, float unscaledHeight, float offsetX = 0, float offsetY = 0)
        {
            // 判断是否在矩形 [0, Width] x [0, Height] 内
            return localPosition.X >= 0 - offsetX && localPosition.Y >= 0 - offsetY && localPosition.X <= unscaledWidth - offsetX && localPosition.Y <= unscaledHeight - offsetY;
        }

        public bool RectangleHitTest(float unscaledWidth, float unscaledHeight, float additionalOffsetX = 0, float additionalOffsetY = 0)
        {
            if (TryToLocal(out var local))
            {
                return RectangleHitTest(local, unscaledWidth, unscaledHeight, OffsetPosition.X + additionalOffsetX, OffsetPosition.Y + additionalOffsetY);
            }

            return false;
        }

        public static bool CircleHitTest(Vector2 localPosition, float radius, float offsetX = 0, float offsetY = 0)
        {
            return Vector2.DistanceSquared(localPosition, new(-offsetX, -offsetY)) <= radius * radius;
        }

        public bool CircleHitTest(float radius, float additionalOffsetX = 0, float additionalOffsetY = 0)
        {
            if (TryToLocal(out var local))
            {
                return CircleHitTest(local, radius, OffsetPosition.X + additionalOffsetX, OffsetPosition.Y + additionalOffsetY);
            }

            return false;
        }
    }
}
