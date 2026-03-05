using SDL;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Cider.Internals
{
    public static class SDLHelpers
    {
        public static void EnsureOnMainThread()
        {
            if (!SDL3.SDL_IsMainThread())
                throw new SDLException("should run on main thread.");
        }

        [DoesNotReturn]
        public static void Throw() =>
            throw new SDLException(SDL3.SDL_GetError());

        public static unsafe T* ThrowIfPtrIsNull<T>(T* ptr) where T : unmanaged
        {
            if (ptr == null)
                throw new SDLException(SDL3.SDL_GetError());
            return ptr;
        }

        public static unsafe T** ThrowIfPtrIsNull<T>(T** ptr) where T : unmanaged
        {
            if (ptr == null)
                throw new SDLException(SDL3.SDL_GetError());
            return ptr;
        }

        public static void ThrowIfTrue([DoesNotReturnIf(true)] bool condition)
        {
            if (condition)
                throw new SDLException(SDL3.SDL_GetError());
        }

        public static void ThrowIfFalse([DoesNotReturnIf(false)] bool condition)
        {
            if (!condition)
                throw new SDLException(SDL3.SDL_GetError());
        }

        public static T ThrowIfNegative<T>(T number) where T : INumber<T>
        {
            if (T.IsNegative(number))
                throw new SDLException(SDL3.SDL_GetError());
            return number;
        }
    }
}
