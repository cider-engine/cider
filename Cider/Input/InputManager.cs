using Cider.Components;
using Cider.Components.In2D;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cider.Input
{
    public delegate void MouseMovedEventHandler(Window? window, in MouseMovedEventArgs args);
    public delegate void MouseButtonEventHandler(Window? window, in MouseButtonEventArgs args);
    public delegate void KeyboardEventHandler(Window? window, in KeyboardEventArgs args);

    public static partial class InputManager
    {
#nullable disable
        public static event MouseMovedEventHandler MouseMoved;

        public static event MouseButtonEventHandler MouseUp;

        public static event MouseButtonEventHandler MouseDown;
#nullable enable
        private static readonly List<Component2D> visitedMouseMovedComponents = new(256); // 深度

        internal static void RaiseMouseMoved(Window? window, in MouseMovedEventArgs args)
        {
            var context = new ComponentEventContext();

            if (window is { Scene: { } scene, Renderer.Camera2D.OffsetPosition: var offset })
            {
                using (var result = HitTestResult.GetScopedSingleton(args.Position - args.Movement, offset))
                {
                    scene.HitTestDispatcher(result);

                    if (result.GetComponent() is Component component)
                    {
                        context.Target = component;

                        foreach (var item in component.EnumerateToRoot())
                        {
                            if (item is Component2D c2d)
                            {
                                c2d.OnMouseMoved(c2d, args, ref context);
                                visitedMouseMovedComponents.Add(c2d);
                            }
                        }
                    }
                }

                var crossIndex = -1;

                using (var result = HitTestResult.GetScopedSingleton(args.Position, offset))
                {
                    window.Scene.HitTestDispatcher(result);

                    if (result.GetComponent() is Component component)
                    {
                        context.Target = component;

                        foreach (var item in component.EnumerateToRoot())
                        {
                            if (item is Component2D c2d)
                            {
                                crossIndex = visitedMouseMovedComponents.IndexOf(c2d);
                                if (crossIndex >= 0) break;

                                c2d.IsMouseOver = true;
                                c2d.OnMouseEnter(c2d, args);

                                c2d.OnMouseMoved(component, args, ref context);
                            }
                        }
                    }
                }

                {
                    // span的生命周期只在这个block内
                    var span = CollectionsMarshal.AsSpan(visitedMouseMovedComponents);

                    // 如果第二次命中测试成功，就遍历到第一个重复父元素前
                    // 如果没成功，就全部遍历
                    foreach (var c2d in crossIndex >= 0 ? span[..crossIndex] : span)
                    {
                        c2d.IsMouseOver = false;
                        c2d.OnMouseLeave(c2d, args);
                    }
                }

                visitedMouseMovedComponents.Clear();
            }

            if (!context.SuppressGlobalHandling)
                MouseMoved?.Invoke(window, args);
        }

        internal static void RaiseMouseUp(Window? window, in MouseButtonEventArgs args)
        {
            var context = new ComponentEventContext();

            if (window is { Scene: { } scene, Renderer.Camera2D.OffsetPosition: var offset })
            {
                using var result = HitTestResult.GetScopedSingleton(args.Position, offset);

                scene.HitTestDispatcher(result);

                if (result.GetComponent() is Component component)
                {
                    context.Target = component;

                    foreach (var item in component.EnumerateToRoot())
                    {
                        if (item is Component2D c2d)
                        {
                            c2d.OnMouseUp(c2d, args, ref context);
                        }
                    }
                }
            }

            if (!context.SuppressGlobalHandling)
                MouseUp?.Invoke(window, args);
        }

        internal static void RaiseMouseDown(Window? window, in MouseButtonEventArgs args)
        {
            var context = new ComponentEventContext();

            if (window is { Scene: { } scene, Renderer.Camera2D.OffsetPosition: var offset })
            {
                using var result = HitTestResult.GetScopedSingleton(args.Position, offset);

                scene.HitTestDispatcher(result);

                if (result.GetComponent() is Component component)
                {
                    context.Target = component;

                    Component2D? focusedComponent = null;

                    foreach (var item in component.EnumerateToRoot())
                    {
                        if (item is Component2D c2d)
                        {
                            c2d.OnMouseDown(c2d, args, ref context);
                            if (c2d.Focusable)
                            {
                                focusedComponent ??= c2d;
                            }
                        }
                    }

                    if (focusedComponent is not null)
                        window.SetFocus(focusedComponent);
                }
            }

            if (!context.SuppressGlobalHandling)
                MouseDown?.Invoke(window, args);
        }

        public static Component2D? FocusedComponent => Keyboard.FocusedWindow?.FocusedComponent;
    }

    partial class InputManager
    {
#nullable disable
        public static event KeyboardEventHandler KeyDown;

        public static event KeyboardEventHandler KeyUp;
#nullable enable
        internal static void RaiseKeyDown(Window? window, in KeyboardEventArgs args)
        {
            var context = new ComponentEventContext();

            if (window is { FocusedComponent: { } component })
            {
                context.Target = component;

                foreach (var item in component.EnumerateToRoot())
                {
                    if (item is Component2D c2d)
                    {
                        c2d.OnKeyDown(c2d, args, ref context);
                    }
                }
            }

            if (!context.SuppressGlobalHandling)
                KeyDown?.Invoke(window, args);
        }

        internal static void RaiseKeyUp(Window? window, in KeyboardEventArgs args)
        {
            var context = new ComponentEventContext();

            if (window is { FocusedComponent: { } component })
            {
                context.Target = component;

                foreach (var item in component.EnumerateToRoot())
                {
                    if (item is Component2D c2d)
                    {
                        c2d.OnKeyUp(c2d, args, ref context);
                    }
                }
            }

            if (!context.SuppressGlobalHandling)
                KeyUp?.Invoke(window, args);
        }
    }

    partial class InputManager
    {
        internal static void Update()
        {
            Mouse.Update();
            Keyboard.Update();
        }

        internal static void FixedUpdate()
        {
            Mouse.FixedUpdate();
            Keyboard.FixedUpdate();
        }
    }
}
