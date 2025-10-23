using Cider.Components;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Input
{
    public class HitTestResult(MouseEventArgs eventArgs)
    {
#nullable enable
        private Component? _component;

        public MouseEventArgs EventArgs => eventArgs;

        public void SetComponent(Component control) => _component = control ?? throw new NullReferenceException();

        public Component? GetComponent() => _component;
    }
}
