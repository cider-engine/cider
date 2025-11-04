using Cider.Attributes;
using System;
using MonoGame.Extended.Input.InputListeners;

namespace Cider.Components.In2D.Controls
{
    public class Button : TextBlock
    {
        protected internal override void OnMouseUp(object sender, MouseEventArgs args)
        {
            OnClick(this, args);
            base.OnMouseUp(sender, args);
        }

        protected virtual void OnClick(object sender, MouseEventArgs args)
        {
            Click?.Invoke(sender, args);
        }

        public event EventHandler<MouseEventArgs> Click;
    }
}
