using Cider.Components;
using Cider.Data.In2D;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Cider.Input
{
    public class HitTestResult : IDisposable
    {
#nullable enable
        private static readonly HitTestResult singleton = new();

        public static HitTestResult GetScopedSingleton(Vector2 position)
        {
            singleton.Position = position;
            return singleton;
        }

        private Component? _component;

        public Transform2D CurrentTransform2D { get; set; } = new();

        public Vector2 Position { get; set; }

        public void SetComponent(Component control) => _component = control ?? throw new NullReferenceException();

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
    }
}
