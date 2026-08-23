using Cider.Data.In2D;
using Cider.Render;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Cider.Components.In2D
{
    public class RemoteCamera2D : Component2D
    {
        private protected override void OnGlobalTransformChangedInternal(EventArgs args)
        {
            if (CurrentWindow?.Renderer.Camera2D is { } camera)
                NotifyNewPosition(camera, ((Transform2DChangedEventArgs)args).CurrentTransform2D.Position);

            base.OnGlobalTransformChangedInternal(args);
        }

        private protected override void OnWindowChangedInternal(Window? oldWindow, Window? newWindow)
        {
            if (oldWindow?.Renderer.Camera2D is { } oldCamera)
            {
                oldCamera.IsEnabled = false;
            }

            if (newWindow?.Renderer.Camera2D is { } newCamera)
            {
                Game.Assert(!newCamera.IsEnabled);

                newCamera.IsEnabled = true;

                NotifyNewPosition(newCamera, GlobalTransform.Position);
            }

            base.OnWindowChangedInternal(oldWindow, newWindow);
        }

        public virtual void NotifyNewPosition(Camera2D camera, Vector2 postion)
        {
            camera.NotifyNewPosition(postion);
        }
    }
}
