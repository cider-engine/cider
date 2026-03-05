using Cider.Components;
using Cider.Components.In2D;
using SDL;
using System;
using System.Collections.Generic;

namespace Cider.Input
{
#nullable enable
    public delegate void MouseMovedEventHandler(Window? window, MouseMovedEventArgs args);
    public delegate void MouseButtonEventHandler(Window? window, MouseButtonEventArgs args);

    public static class InputManager
    {
#nullable disable
        public static event MouseMovedEventHandler MouseMoved;

        public static event MouseButtonEventHandler MouseUp;

        public static event MouseButtonEventHandler MouseDown;
#nullable enable
        private static readonly HashSet<Component2D> visitedMouseMovedComponents = new(256); // 深度

        internal static void RaiseMouseMoved(Window? window, in SDL_MouseMotionEvent e)
        {
            var args = new MouseMovedEventArgs(
                Position: new(e.x, e.y),
                Movement: new(e.xrel, e.yrel),
                Timestamp: e.timestamp,
                MouseId: e.which,
                ButtonState: (MouseButtonFlags)e.state);

            MouseMoved?.Invoke(window, args);

            if (window is null) return;

            Component2D? mouseLeave = null;

            Component2D? mouseEnter = null;

            using (var result = HitTestResult.GetScopedSingleton(args.Position - args.Movement))
            {
                window.Scene.HitTestDispatcher(result);

                if (result.GetComponent() is Component component)
                {
                    mouseLeave = component as Component2D;
                    foreach (var item in component.EnumerateToRoot())
                    {
                        if (item is Component2D c2d)
                        {
                            c2d.OnMouseMoved(component, args);
                            visitedMouseMovedComponents.Add(c2d);
                        }
                    }
                }
            }

            using (var result = HitTestResult.GetScopedSingleton(args.Position))
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
                            c2d.OnMouseMoved(component, args);
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

        internal static void RaiseMouseUp(Window? window, in SDL_MouseButtonEvent e)
        {
            var args = new MouseButtonEventArgs(
                Position: new(e.x, e.y),
                Timestamp: e.timestamp,
                MouseId: e.which,
                Button: (MouseButton)e.button,
                IsDown: e.down,
                ClickTimes: e.clicks
            );

            MouseUp?.Invoke(window, args);

            if (window is null) return;

            using var result = HitTestResult.GetScopedSingleton(args.Position);

            window.Scene.HitTestDispatcher(result);

            if (result.GetComponent() is Component component)
            {
                foreach (var item in component.EnumerateToRoot())
                {
                    if (item is Component2D c2d)
                    {
                        c2d.OnMouseUp(component, args);
                    }
                }
            }
        }

        internal static void RaiseMouseDown(Window? window, in SDL_MouseButtonEvent e)
        {
            var args = new MouseButtonEventArgs(
                Position: new(e.x, e.y),
                Timestamp: e.timestamp,
                MouseId: e.which,
                Button: (MouseButton)e.button,
                IsDown: e.down,
                ClickTimes: e.clicks
            );

            MouseDown?.Invoke(window, args);

            if (window is null) return;

            using var result = HitTestResult.GetScopedSingleton(args.Position);

            window.Scene.HitTestDispatcher(result);

            if (result.GetComponent() is Component component)
            {
                foreach (var item in component.EnumerateToRoot())
                {
                    if (item is Component2D c2d)
                    {
                        c2d.OnMouseDown(component, args);
                    }
                }
            }
        }
    }
}
