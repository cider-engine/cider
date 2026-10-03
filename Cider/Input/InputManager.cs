using Cider.Components;
using Cider.Components.In2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static readonly List<Component2D> currentMouseOverComponents = new(16); // 深度

        internal static void RaiseMouseMoved(Window? window, in MouseMovedEventArgs args)
        {
            var context = new ComponentEventContext();

            if (window is { Scene: { } scene, Renderer.Camera2D.OffsetPosition: var offset })
            {
                using (var result = HitTestResult.GetScopedSingleton(args.Position, offset))
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
                                currentMouseOverComponents.Add(c2d);
                            }
                        }
                    }
                }

                foreach (var item in scene.LastMouseOverComponents)
                {
                    if (!currentMouseOverComponents.Contains(item))
                    {
                        Debug.Assert(item.IsMouseOver);
                        item.IsMouseOver = false;
                        item.OnMouseLeave(item, args);
                    }
                }

                foreach (var item in currentMouseOverComponents)
                {
                    if (!item.IsMouseOver)
                    {
                        item.IsMouseOver = true;
                        item.OnMouseEnter(item, args);
                    }
                }

                // 限制在当前block中
                {
                    CollectionsMarshal.SetCount(scene.LastMouseOverComponents, currentMouseOverComponents.Count);
                    var currentMouseOverSpan = CollectionsMarshal.AsSpan(currentMouseOverComponents);
                    var lastMouseOverSpan = CollectionsMarshal.AsSpan(scene.LastMouseOverComponents);
                    currentMouseOverSpan.CopyTo(lastMouseOverSpan);
                }

                currentMouseOverComponents.Clear();
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
