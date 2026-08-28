using Cider.Attributes;
using Cider.Data.In2D;
using Cider.Input;
using System;
using System.Diagnostics;
using System.Numerics;

namespace Cider.Components.In2D
{
    public struct ComponentEventContext
    {
        /// <summary>
        /// 是否抑制全局处理，将此属性设为true来阻止InputManager调用全局事件处理器
        /// </summary>
        public bool SuppressGlobalHandling { get; set; }
    }
    public delegate void ComponentMouseMovedEventHandler(Component sender, in MouseMovedEventArgs args, ref ComponentEventContext context);
    public delegate void ComponentMouseEnterEventHandler(Component sender, in MouseMovedEventArgs args);
    public delegate void ComponentMouseLeaveEventHandler(Component sender, in MouseMovedEventArgs args);
    public delegate void ComponentMouseButtonEventHandler(Component sender, in MouseButtonEventArgs args, ref ComponentEventContext context);
    public delegate void ComponentKeyboardEventHandler(Component sender, in KeyboardEventArgs args, ref ComponentEventContext context);
    public delegate void GotFocusEventHandler(Component2D sender, Component2D? lostFocusComponent);
    public delegate void LostFocusEventHandler(Component2D sender, Component2D? gotFocusComponent);

    /// <summary>
    /// <para>2D组件的基类</para>
    /// </summary>
    public class Component2D : Component
    {
        /// <summary>
        /// 当前的局部位置，Transform.Position的别名
        /// </summary>
        public Vector2 Position
        {
            get => Transform.Position;
            set => Transform = new Transform2D(value, Transform.RotationInRadians, Transform.Scale);
        }

        /// <summary>
        /// 当前的局部缩放，Transform.Scale的别名
        /// </summary>
        public Vector2 Scale
        {
            get => Transform.Scale;
            set => Transform = new Transform2D(Transform.Position, Transform.RotationInRadians, value);
        }

        /// <summary>
        /// 当前的局部旋转，Transform.RotationInRadians的别名，单位为弧度
        /// </summary>
        public float RotationInRadians
        {
            get => Transform.RotationInRadians;
            set => Transform = new Transform2D(Transform.Position, value, Transform.Scale);
        }

        /// <summary>
        /// 当前的局部旋转，Transform.RotationInDegrees的别名，单位为角度
        /// </summary>
        public float RotationInDegrees
        {
            get => RotationInRadians * (180 / MathF.PI);
            set => RotationInRadians = value * (MathF.PI / 180);
        }

        /// <summary>
        /// 当前的局部变换
        /// </summary>
        public Transform2D Transform
        {
            get;
            set
            {
                field = value;
                var args = new Transform2DChangedEventArgs()
                {
                    CurrentTransform2D = GlobalTransform
                };
                OnGlobalTransformChangedInternal(args);
                var toBeRestored = args.CurrentTransform2D;
                foreach (var item in Children)
                {
                    item.OnGlobalTransformChangedDispatcher(args);
                    args.CurrentTransform2D = toBeRestored;
                }
            }
        } = new();

        private protected Transform2D _parentGlobalTransform = new();

        /// <summary>
        /// <para>当前的全局变换</para>
        /// <para>尽管这个属性的开销较小，在单次使用中仍建议将此属性的计算结果存为局部变量并复用</para>
        /// </summary>
        public Transform2D GlobalTransform
        {
            get => _parentGlobalTransform.ApplyTransform2D(Transform);
        }

        /// <summary>
        /// 自身或任意一个子组件是否在鼠标下方
        /// </summary>
        public bool IsMouseOver { get; internal set; }

        /// <summary>
        /// 是否被聚焦，一个窗口只可能有一个被聚焦组件
        /// </summary>
        public bool IsFocused { get; internal set; }

        /// <summary>
        /// 是否可聚焦，当this.IsFocused为true时将此属性改为false会同步更改
        /// </summary>
        public bool Focusable
        {
            get;
            set
            {
                field = value;
                if (IsFocused)
                {
                    Debug.Assert(CurrentWindow is not null);
                    CurrentWindow?.ClearFocus();
                }
            }
        }
#nullable disable
        public event ComponentMouseButtonEventHandler MouseDown;

        public event ComponentMouseButtonEventHandler MouseUp;

        public event ComponentMouseMovedEventHandler MouseMoved;

        public event ComponentMouseEnterEventHandler MouseEnter;

        public event ComponentMouseLeaveEventHandler MouseLeave;

        public event ComponentKeyboardEventHandler KeyDown;

        public event ComponentKeyboardEventHandler KeyUp;

        public event GotFocusEventHandler GotFocus;

        public event LostFocusEventHandler LostFocus;
#nullable restore
        private protected override Transform2DChangedEventArgs CreateGlobalTransformArgsFromCurrent() => new()
        {
            CurrentTransform2D = GlobalTransform
        };

        internal override void OnGlobalTransformChangedDispatcher(EventArgs args)
        {
            if (args is Transform2DChangedEventArgs args2D)
            {
                _parentGlobalTransform = args2D.CurrentTransform2D;
                args2D.ApplyTransform(Transform);
                OnGlobalTransformChangedInternal(args2D);

                var toBeRestored = args2D.CurrentTransform2D;
                foreach (var item in Children)
                {
                    item.OnGlobalTransformChangedDispatcher(args2D);
                    args2D.CurrentTransform2D = toBeRestored;
                }
            }

            else
            {
                _parentGlobalTransform = new();
                var newArgs2D = new Transform2DChangedEventArgs()
                {
                    CurrentTransform2D = Transform // 重置变换链
                };
                OnGlobalTransformChangedInternal(newArgs2D);

                var toBeRestored = newArgs2D.CurrentTransform2D;
                foreach (var item in Children)
                {
                    item.OnGlobalTransformChangedDispatcher(newArgs2D);
                    newArgs2D.CurrentTransform2D = toBeRestored;
                }
            }
        }

        [Dispatcher]
        internal override void HitTestDispatcher(HitTestResult result)
        {
            if (!IsVisible) return;

            result.ApplyTransform(Transform);
            var toBeRestored = result.CurrentTransform2D;

            if (HitTest(result)) result.SetComponent(this);

            foreach (var item in Children)
            {
                item.HitTestDispatcher(result);
                result.CurrentTransform2D = toBeRestored;
            }
        }

        private protected override void OnWindowChangedInternal(Window? oldWindow, Window? newWindow)
        {
            if (IsFocused)
            {
                Debug.Assert(oldWindow?.FocusedComponent == this);
                oldWindow?.ClearFocus();
            }
            base.OnWindowChangedInternal(oldWindow, newWindow);
        }

        /// <summary>
        /// <para>当鼠标在此组件或子组件上被按下时触发的回调函数</para>
        /// <para>此函数同时还会触发MouseDown事件，覆写此函数时可通过base关键字控制该行为</para>
        /// <para>此函数是否被调用依赖HitTest的返回结果</para>
        /// </summary>
        /// <param name="sender">此事件的源</param>
        /// <param name="args"></param>
        /// <param name="context">可通过此参数禁止InputManager的全局事件处理器被调用</param>
        protected internal virtual void OnMouseDown(Component sender, in MouseButtonEventArgs args, ref ComponentEventContext context)
        {
            MouseDown?.Invoke(sender, args, ref context);
        }

        /// <summary>
        /// <para>当鼠标在此组件或子组件上被放开时触发的回调函数</para>
        /// <para>此函数同时还会触发MouseUp事件，覆写此函数时可通过base关键字控制该行为</para>
        /// <para>此函数是否被调用依赖HitTest的返回结果</para>
        /// </summary>
        /// <param name="sender">此事件的源</param>
        /// <param name="args"></param>
        /// <param name="context">可通过此参数禁止InputManager的全局事件处理器被调用</param>
        protected internal virtual void OnMouseUp(Component sender, in MouseButtonEventArgs args, ref ComponentEventContext context)
        {
            MouseUp?.Invoke(sender, args, ref context);
        }

        /// <summary>
        /// <para>当鼠标在此组件或子组件上移动时触发的回调函数</para>
        /// <para>此函数同时还会触发MouseMoved事件，覆写此函数时可通过base关键字控制该行为</para>
        /// <para>此函数是否被调用依赖HitTest的返回结果</para>
        /// </summary>
        /// <param name="sender">此事件的源</param>
        /// <param name="args"></param>
        /// <param name="context">可通过此参数禁止InputManager的全局事件处理器被调用</param>
        protected internal virtual void OnMouseMoved(Component sender, in MouseMovedEventArgs args, ref ComponentEventContext context)
        {
            MouseMoved?.Invoke(sender, args, ref context);
        }

        /// <summary>
        /// <para>当鼠标进入此组件或子组件上时触发的回调函数</para>
        /// <para>此函数同时还会触发MouseEnter事件，覆写此函数时可通过base关键字控制该行为</para>
        /// <para>此函数是否被调用依赖HitTest的返回结果</para>
        /// </summary>
        /// <param name="sender">此事件的源</param>
        /// <param name="args"></param>
        protected internal virtual void OnMouseEnter(Component sender, in MouseMovedEventArgs args)
        {
            MouseEnter?.Invoke(sender, args);
        }

        /// <summary>
        /// <para>当鼠标离开此组件或子组件上时触发的回调函数</para>
        /// <para>此函数同时还会触发MouseLeave事件，覆写此函数时可通过base关键字控制该行为</para>
        /// <para>此函数是否被调用依赖HitTest的返回结果</para>
        /// </summary>
        /// <param name="sender">此事件的源</param>
        /// <param name="args"></param>
        protected internal virtual void OnMouseLeave(Component sender, in MouseMovedEventArgs args)
        {
            MouseLeave?.Invoke(sender, args);
        }

        /// <summary>
        /// <para>当此组件或子组件在被聚焦时按下键盘按键触发的回调函数</para>
        /// <para>此函数同时还会触发KeyDown事件，覆写此函数时可通过base关键字控制该行为</para>
        /// </summary>
        /// <param name="sender">此事件的源</param>
        /// <param name="args"></param>
        /// <param name="context">可通过此参数禁止InputManager的全局事件处理器被调用</param>
        protected internal virtual void OnKeyDown(Component2D sender, in KeyboardEventArgs args, ref ComponentEventContext context)
        {
            KeyDown?.Invoke(sender, args, ref context);
        }

        /// <summary>
        /// <para>当此组件或子组件在被聚焦时放开键盘按键触发的回调函数</para>
        /// <para>此函数同时还会触发KeyUp事件，覆写此函数时可通过base关键字控制该行为</para>
        /// </summary>
        /// <param name="sender">此事件的源</param>
        /// <param name="args"></param>
        /// <param name="context">可通过此参数禁止InputManager的全局事件处理器被调用</param>
        protected internal virtual void OnKeyUp(Component sender, in KeyboardEventArgs args, ref ComponentEventContext context)
        {
            KeyUp?.Invoke(sender, args, ref context);
        }

        /// <summary>
        /// <para>当此组件获取焦点时触发的回调函数</para>
        /// <para>此函数同时还会触发GotFocus事件，覆写此函数时可通过base关键字控制该行为</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="lostFocusComponent"></param>
        protected internal virtual void OnGotFocus(Component2D sender, Component2D? lostFocusComponent)
        {
            GotFocus?.Invoke(sender, lostFocusComponent);
        }

        /// <summary>
        /// <para>当此组件失去焦点时触发的回调函数</para>
        /// <para>此函数同时还会触发LostFocus事件，覆写此函数时可通过base关键字控制该行为</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="gotFocusComponent"></param>
        protected internal virtual void OnLostFocus(Component2D sender, Component2D? gotFocusComponent)
        {
            LostFocus?.Invoke(sender, gotFocusComponent);
        }
    }
}
