using SDL;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Cider.Input
{
    public static class Mouse
    {
        public static MouseButtonFlags GetState(out float x, out float y)
        {
            unsafe
            {
                fixed (float* _x = &x, _y = &y)
                    return (MouseButtonFlags)SDL3.SDL_GetMouseState(_x, _y);
            }
        }
    }

    public readonly record struct MouseMovedEventArgs(Vector2 Position,
        Vector2 Movement,
        ulong Timestamp,
        MouseId MouseId,
        MouseButtonFlags ButtonState);

    public readonly record struct MouseButtonEventArgs(Vector2 Position,
        ulong Timestamp,
        MouseId MouseId,
        MouseButton Button,
        bool IsDown,
        byte ClickTimes);

    public readonly record struct MouseId(uint Id)
    {
        public const uint TouchId = unchecked((uint)-1);
        public readonly bool IsTouch => Id == TouchId;
        public static implicit operator MouseId(SDL_MouseID id) => new((uint)id);
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
