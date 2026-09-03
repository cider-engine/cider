using Cider.Data;
using Cider.Data.In2D;
using Cider.Render;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Cider.Components.In2D
{
    /// <summary>
    /// <para>远程相机组件，将此组件添加到树中自动通知相机变化</para>
    /// </summary>
    public class RemoteCamera2D : Component2D
    {
        private CameraPosition _targetPosition = default;
        private CameraPosition _currentPosition = default;

        public bool IsSmooth { get; set; }

        public double Speed { get; set => field = double.IsNegative(value) ? throw new ArgumentOutOfRangeException(nameof(Speed)) : value; } = 5;

        /// <summary>
        /// <para>远程相机的更新模式，决定了位置通知是在渲染帧或物理帧中进行</para>
        /// <para>这个属性的值应与物体的运动方式一致</para>
        /// <para>当平滑移动出现拖影或模糊时，可以尝试更改这个属性</para>
        /// </summary>
        public CameraUpdateMode UpdateMode { get; set => field = Enum.IsDefined(value) ? value : throw new ArgumentException("Invalid mode"); } = CameraUpdateMode.Update;

        private protected override void OnGlobalTransformChangedInternal(EventArgs args)
        {
            var position = ((Transform2DChangedEventArgs)args).CurrentTransform2D.Position;

            OnNewTargetPosition(new(position.X, position.Y));

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
                Debug.Assert(!newCamera.IsEnabled);

                newCamera.IsEnabled = true;

                var position = GlobalTransform.Position;

                OnNewTargetPosition(new(position.X, position.Y));
            }

            base.OnWindowChangedInternal(oldWindow, newWindow);
        }

        private protected override void OnUpdateInternal(TimeContext context)
        {
            if (UpdateMode == CameraUpdateMode.Update) OnCameraUpdate(context, ref _currentPosition, _targetPosition);
            base.OnUpdateInternal(context);
        }

        private protected override void OnFixedUpdateInternal(TimeContext context)
        {
            if (UpdateMode == CameraUpdateMode.FixedUpdate) OnCameraUpdate(context, ref _currentPosition, _targetPosition);
            base.OnFixedUpdateInternal(context);
        }

        protected virtual void OnNewTargetPosition(CameraPosition position)
        {
            _targetPosition = position;
        }

        protected virtual void OnCameraUpdate(TimeContext context, ref CameraPosition currentPosition, CameraPosition targetPosition)
        {
            if (currentPosition != targetPosition)
            {
                if (IsSmooth)
                {
                    currentPosition = Lerp(currentPosition, targetPosition, double.Clamp(Speed * context.DeltaTime.TotalSeconds, 0, 1));
                    NotifyNewPosition(CurrentWindow!.Renderer.Camera2D, currentPosition);
                }

                else
                {
                    currentPosition = targetPosition;
                    NotifyNewPosition(CurrentWindow!.Renderer.Camera2D, currentPosition);
                }
            }
        }

        public virtual void NotifyNewPosition(Camera2D camera, CameraPosition postion)
        {
            var (x, y) = postion;
            camera.NotifyNewPosition(new((float)x, (float)y));
        }

        static CameraPosition Lerp(CameraPosition a, CameraPosition b, double t)
        {
            var x = a.X + (b.X - a.X) * t;
            var y = a.Y + (b.Y - a.Y) * t;
            return new(x, y);
        }
    }

    public enum CameraUpdateMode
    {
        /// <summary>
        /// 远程相机在渲染帧中
        /// </summary>
        Update,
        /// <summary>
        /// 远程相机在物理帧中
        /// </summary>
        FixedUpdate
    }

    public record struct CameraPosition(double X, double Y);
}
