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

        public static HitTestResult GetScopedSingleton(Vector2 position)
        {
            singleton.Position = position;
            return singleton;
        }

        private Component? _component;

        public Transform2D CurrentTransform2D { get; set; } = new();

        public Vector2 Position { get; set; }

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

        public bool RectangleHitTest(float unscaledWidth, float unscaledHeight, float offsetX = 0, float offsetY = 0)
        {
            if (TryToLocal(out var local))
            {
                // 判断是否在矩形 [0, Width] x [0, Height] 内
                if (local.X >= 0 - offsetX && local.Y >= 0 - offsetY && local.X <= unscaledWidth - offsetX && local.Y <= unscaledHeight - offsetY)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
