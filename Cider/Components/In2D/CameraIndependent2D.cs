using Cider.Input;
using Cider.Render;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Components.In2D
{
    /// <summary>
    /// <para>与相机无关的组件，此组件的所有子组件不会受到相机的影响</para>
    /// <para>由于整个场景只有一个物理世界，混用受或不受相机影响的物理对象会导致渲染问题</para>
    /// </summary>
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
