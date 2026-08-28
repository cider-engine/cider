using Cider.Extensions;
using Cider.Internals;
using Cider.Render;
using SDL;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Cider.Input
{
    public static class Mouse
    {
        private static MouseButtonFlags _lastState;
        private static MouseButtonFlags _lastPhysicsState;

        /// <summary>
        /// 返回当前的鼠标状态
        /// </summary>
        /// <returns></returns>
        public static unsafe MouseButtonFlags GetState()
        {
            return (MouseButtonFlags)SDL3.SDL_GetMouseState(null, null);
        }

        public static unsafe MouseButtonFlags GetState(out Vector2 position)
        {
            if (FocusedWindow is { } window)
            {
                float x, y;
                var flags = (MouseButtonFlags)SDL3.SDL_GetMouseState(&x, &y);

                fixed (Vector2* ptr = &position)
                    SDLHelpers.ThrowIfFalse(SDL3.SDL_RenderCoordinatesFromWindow(window.Renderer.Pointer, x, y, &ptr->X, &ptr->Y));

                return flags;
            }

            else
            {
                fixed (Vector2* ptr = &position)
                    return (MouseButtonFlags)SDL3.SDL_GetMouseState(&ptr->X, &ptr->Y);
            }
        }

        /// <summary>
        /// <para>返回上一帧的鼠标状态</para>
        /// <para>如果当前在物理帧中，返回上一物理帧的鼠标状态</para>
        /// <para>如果当前不在物理帧中，返回上一渲染帧的鼠标状态</para>
        /// </summary>
        /// <returns></returns>
        public static MouseButtonFlags GetLastState() => Game.Instance.IsInPhysicsFrame ? _lastPhysicsState : _lastState;

        public static unsafe Window? FocusedWindow => SDL3.SDL_GetMouseFocus() is not null and var ptr ? SDL3.SDL_GetWindowID(ptr).RelativeWindow : null;

        internal static unsafe void Update()
        {
            _lastState = (MouseButtonFlags)SDL3.SDL_GetMouseState(null, null);
        }

        internal static unsafe void FixedUpdate()
        {
            _lastPhysicsState = (MouseButtonFlags)SDL3.SDL_GetMouseState(null, null);
        }

        /// <summary>
        /// 获取当前鼠标在窗口上的原位置
        /// </summary>
        /// <param name="renderer">窗口所使用的渲染器</param>
        /// <returns>鼠标位置</returns>
        public static unsafe Vector2 GetRawMousePosition(Renderer renderer)
        {
            GetState(out var position);

            Vector2 vector = Vector2.Zero;

            SDLHelpers.ThrowIfFalse(SDL3.SDL_RenderCoordinatesToWindow(renderer.Pointer, position.X, position.Y, &vector.X, &vector.Y));

            return vector;
        }

        public static bool IsPressed(MouseButtonFlags buttons)
        {
            return (GetState() & buttons) != 0;
        }

        public static bool IsReleased(MouseButtonFlags buttons)
        {
            return (GetState() & buttons) == 0;
        }

        public static bool IsJustPressed(MouseButtonFlags buttons)
        {
            return ((GetLastState() & buttons) == 0) && ((GetState() & buttons) != 0);
        }

        public static bool IsJustReleased(MouseButtonFlags buttons)
        {
            return ((GetLastState() & buttons) != 0) && ((GetState() & buttons) == 0);
        }

        static Mouse()
        {
            Update();
            FixedUpdate();
        }
    }

    public readonly record struct MouseMovedEventArgs(Vector2 Position,
        Vector2 RawPosition,
        Vector2 Movement,
        Vector2 RawMovement,
        GameTimestamp Timestamp,
        MouseId MouseId,
        MouseButtonFlags ButtonState);

    public readonly record struct MouseButtonEventArgs(Vector2 Position,
        Vector2 RawPosition,
        GameTimestamp Timestamp,
        MouseId MouseId,
        MouseButton Button,
        bool IsDown,
        byte ClickTimes);

    public readonly record struct MouseId(uint Id)
    {
        public const uint TouchId = unchecked((uint)-1);
        public readonly bool IsTouch => Id == TouchId;
        public readonly bool IsInvalid => Id == 0;
    }

    public enum MouseButton
    {
        Left = SDL3.SDL_BUTTON_LEFT,
        Middle = SDL3.SDL_BUTTON_MIDDLE,
        Right = SDL3.SDL_BUTTON_RIGHT,
        X1 = SDL3.SDL_BUTTON_X1,
        X2 = SDL3.SDL_BUTTON_X2
    }

    [Flags]
    public enum MouseButtonFlags : uint
    {
        Left = SDL_MouseButtonFlags.SDL_BUTTON_LMASK,
        Middle = SDL_MouseButtonFlags.SDL_BUTTON_MMASK,
        Right = SDL_MouseButtonFlags.SDL_BUTTON_RMASK,
        X1 = SDL_MouseButtonFlags.SDL_BUTTON_X1MASK,
        X2 = SDL_MouseButtonFlags.SDL_BUTTON_X2MASK
    }
}
