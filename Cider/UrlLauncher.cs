using Cider.Extensions;
using Cider.Internals;
using SDL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider
{
    public static class UrlLauncher
    {
        public static unsafe void OpenUrl(string url)
        {
            using var unmanaged = url.ToUnmanagedUtf8();
            SDLHelpers.ThrowIfFalse(SDL3.SDL_OpenURL(unmanaged.Pointer));
        }
    }
}
