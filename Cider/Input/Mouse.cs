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

        public static unsafe Window? FocusedWindow => SDL3.SDL_GetMouseFocus() is not null and var ptr ? SDL3.SDL_GetWindowID(ptr).RelativeWindow : null;

        internal static unsafe void Update()
        {
            _lastState = (MouseButtonFlags)SDL3.SDL_GetMouseState(null, null);
        }

        public static unsafe Vector2 GetRawMousePosition(Vector2 position, Renderer renderer)
        {
            Vector2 vector = Vector2.Zero;

            SDLHelpers.ThrowIfFalse(SDL3.SDL_RenderCoordinatesToWindow(renderer.Pointer, position.X, position.Y, &vector.X, &vector.Y));

            return vector;
        }

        static Mouse()
        {
            Update();
        }
    }

    public readonly record struct MouseMovedEventArgs(Vector2 Position,
        Vector2 RawPosition,
        Vector2 Movement,
        Vector2 RawMovement,
        ulong Timestamp,
        MouseId MouseId,
        MouseButtonFlags ButtonState);

    public readonly record struct MouseButtonEventArgs(Vector2 Position,
        Vector2 RawPosition,
        ulong Timestamp,
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
