using Cider.Input;
using System;

namespace Cider.Components.In2D.Controls
{
    public class Button : TextBlock
    {
        protected internal override void OnMouseUp(Component sender, MouseButtonEventArgs args)
        {
            OnClick(this, args);
            base.OnMouseUp(sender, args);
        }

        protected virtual void OnClick(Component sender, MouseButtonEventArgs args)
        {
            Click?.Invoke(sender, args);
        }

        public event EventHandler<Component, MouseButtonEventArgs> Click;
    }
}
