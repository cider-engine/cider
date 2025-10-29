using Cider.Components;
using Cider.Data.In2D;
using MonoGame.Extended.Input.InputListeners;
using System;

namespace Cider.Input
{
    public class HitTestResult(MouseEventArgs eventArgs)
    {
#nullable enable
        private Component? _component;

        public Transform2D CurrentTransform2D { get; set; } = new();

        public MouseEventArgs EventArgs => eventArgs;

        public void SetComponent(Component control) => _component = control ?? throw new NullReferenceException();

        public Component? GetComponent() => _component;

        public HitTestResult ApplyTransform(Transform2D transform)
        {
            CurrentTransform2D = CurrentTransform2D.ApplyTransform2D(transform);
            return this;
        }
    }
}
