using Cider.Components;
using Cider.Components.In2D;
using System;
using System.Collections.Generic;

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
        private static readonly HashSet<Component2D> visitedMouseMovedComponents = new(256); // 深度

        internal static void RaiseMouseMoved(Window? window, in MouseMovedEventArgs args)
        {
            var context = new ComponentEventContext();

            if (window is { Scene: { } scene, Renderer.Camera2D.OffsetPosition: var offset })
            {
                Component2D? mouseLeave = null;

                Component2D? mouseEnter = null;

                using (var result = HitTestResult.GetScopedSingleton(args.Position - args.Movement, offset))
                {
                    scene.HitTestDispatcher(result);

                    if (result.GetComponent() is Component component)
                    {
                        mouseLeave = component as Component2D;
                        foreach (var item in component.EnumerateToRoot())
                        {
                            if (item is Component2D c2d)
                            {
                                c2d.OnMouseMoved(component, args, ref context);
                                visitedMouseMovedComponents.Add(c2d);
                            }
                        }
                    }
                }

                using (var result = HitTestResult.GetScopedSingleton(args.Position, offset))
                {
                    window.Scene.HitTestDispatcher(result);

                    if (result.GetComponent() is Component component)
                    {
                        mouseEnter = component as Component2D;
                        foreach (var item in component.EnumerateToRoot())
                        {
                            if (item is Component2D c2d)
                            {
                                if (visitedMouseMovedComponents.Contains(c2d)) break;
                                c2d.OnMouseMoved(component, args, ref context);
                            }
                        }
                    }
                }

                visitedMouseMovedComponents.Clear();

                if (mouseLeave != mouseEnter)
                {
                    mouseLeave?.IsMouseOver = false;
                    mouseLeave?.OnMouseLeave(mouseLeave, args);

                    mouseEnter?.IsMouseOver = true;
                    mouseEnter?.OnMouseEnter(mouseEnter, args);
                }
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
                    foreach (var item in component.EnumerateToRoot())
                    {
                        if (item is Component2D c2d)
                        {
                            c2d.OnMouseUp(component, args, ref context);
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
                    Component2D? focusedComponent = null;

                    foreach (var item in component.EnumerateToRoot())
                    {
                        if (item is Component2D c2d)
                        {
                            c2d.OnMouseDown(component, args, ref context);
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
                foreach (var item in component.EnumerateToRoot())
                {
                    if (item is Component2D c2d)
                    {
                        c2d.OnKeyDown(component, args, ref context);
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
                foreach (var item in component.EnumerateToRoot())
                {
                    if (item is Component2D c2d)
                    {
                        c2d.OnKeyUp(component, args, ref context);
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
