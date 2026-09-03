using SDL;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Cider.Internals
{
    internal static class SDLHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="GameRuntimeException"></exception>
        public static void EnsureOnMainThread()
        {
            if (!SDL3.SDL_IsMainThread())
                throw new GameRuntimeException("should run on main thread.");
        }

        public static bool TryGetError([NotNullWhen(true)] out GameRuntimeException? exception)
        {
            if (SDL3.SDL_GetError() is string error)
            {
                exception = new GameRuntimeException(error);
                return true;
            }

            else
            {
                exception = null;
                return false;
            }
        }

        public static GameRuntimeException GetError() => new(SDL3.SDL_GetError() ?? "");

        [DoesNotReturn]
        public static void Throw() =>
            throw new GameRuntimeException(SDL3.SDL_GetError() ?? "");

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ptr"></param>
        /// <returns></returns>
        /// <exception cref="GameRuntimeException"></exception>
        public static unsafe T* ThrowIfPtrIsNull<T>(T* ptr) where T : unmanaged
        {
            if (ptr == null)
                throw new GameRuntimeException(SDL3.SDL_GetError() ?? "");
            return ptr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ptr"></param>
        /// <returns></returns>
        /// <exception cref="GameRuntimeException"></exception>
        public static unsafe T** ThrowIfPtrIsNull<T>(T** ptr) where T : unmanaged
        {
            if (ptr == null)
                throw new GameRuntimeException(SDL3.SDL_GetError() ?? "");
            return ptr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="GameRuntimeException"></exception>
        public static T ThrowIfNull<T>(T? obj) where T : class
        {
            if (obj is null)
                throw new GameRuntimeException(SDL3.SDL_GetError() ?? "");
            return obj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <exception cref="GameRuntimeException"></exception>
        public static void ThrowIfTrue([DoesNotReturnIf(true)] bool condition)
        {
            if (condition)
                throw new GameRuntimeException(SDL3.SDL_GetError() ?? "");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <exception cref="GameRuntimeException"></exception>
        public static void ThrowIfFalse([DoesNotReturnIf(false)] bool condition)
        {
            if (!condition)
                throw new GameRuntimeException(SDL3.SDL_GetError() ?? "");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="number"></param>
        /// <returns></returns>
        /// <exception cref="GameRuntimeException"></exception>
        public static T ThrowIfNegative<T>(T number) where T : INumber<T>
        {
            if (T.IsNegative(number))
                throw new GameRuntimeException(SDL3.SDL_GetError() ?? "");
            return number;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="number"></param>
        /// <returns></returns>
        /// <exception cref="GameRuntimeException"></exception>
        public static T ThrowIfZero<T>(T number) where T : INumber<T>
        {
            if (T.IsZero(number))
                throw new GameRuntimeException(SDL3.SDL_GetError() ?? "");
            return number;
        }
    }
}
