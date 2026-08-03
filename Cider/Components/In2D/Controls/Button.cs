using Cider.Input;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using System;

namespace Cider.Components.In2D.Controls
{
    public class Button : TextBlock
    {
        protected internal override void OnMouseUp(Component sender, in MouseButtonEventArgs args, ref ComponentEventContext context)
        {
            OnClick(this, args, ref context);
            base.OnMouseUp(sender, args, ref context);
        }

        protected virtual void OnClick(Component sender, in MouseButtonEventArgs args, ref ComponentEventContext context)
        {
            Click?.Invoke(sender, args, ref context);
        }
#nullable disable
        public event ComponentMouseButtonEventHandler Click;
    }
}
