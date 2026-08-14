using Cider.Input;
using System;

namespace Cider.Components.In2D.Controls
{
    public enum ClickMode
    {
        Press,
        Release
    }

    public abstract class ButtonBase : Control
    {
        public ClickMode ClickMode { get; set; } = ClickMode.Release;

        protected internal override void OnMouseDown(Component sender, in MouseButtonEventArgs args, ref ComponentEventContext context)
        {
            if (ClickMode == ClickMode.Press) OnClick(this, args, ref context);
            base.OnMouseDown(sender, args, ref context);
        }

        protected internal override void OnMouseUp(Component sender, in MouseButtonEventArgs args, ref ComponentEventContext context)
        {
            if (ClickMode == ClickMode.Release) OnClick(this, args, ref context);
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
