using Cider.Input;
using Cider.Render;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Components.In2D
{
    public class CameraIndependent2D : Component2D
    {
        internal override void OnRenderDispatcher(RenderContext context)
        {
            if (!IsVisible) return;

            var isEnabled = context.Renderer.Camera2D.IsEnabled;
            context.Renderer.Camera2D.IsEnabled = false;

            OnRender(context);
            foreach (var item in Children)
                item.OnRenderDispatcher(context);

            context.Renderer.Camera2D.IsEnabled = isEnabled;
        }

        internal override void HitTestDispatcher(HitTestResult result)
        {
            if (!IsVisible) return;

            var offset = result.OffsetPosition;
            result.OffsetPosition = default;

            result.ApplyTransform(Transform);
            var toBeRestored = result.CurrentTransform2D;

            if (HitTest(result)) result.SetComponent(this);

            foreach (var item in Children)
            {
                item.HitTestDispatcher(result);
                result.CurrentTransform2D = toBeRestored;
            }

            result.OffsetPosition = offset;
        }
    }
}
