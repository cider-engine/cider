using Cider.Extensions;
using Cider.Internals;
using SDL;
using System;

namespace Cider
{
    public static class UrlLauncher
    {
        public static unsafe void OpenUrl(string url)
        {
            using var unmanaged = url.ToUnmanagedUtf8();
            SDLHelpers.ThrowIfFalse(SDL3.SDL_OpenURL(unmanaged.Pointer));
        }
        public static unsafe void OpenUrl(ReadOnlySpan<byte> urlNullTerminatedUtf8)
        {
            fixed (byte* ptr = urlNullTerminatedUtf8)
                SDLHelpers.ThrowIfFalse(SDL3.SDL_OpenURL(ptr));
        }
    }
}
