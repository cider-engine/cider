using SDL;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Cider.Extensions
{
    public static class EnumExtensions
    {
        extension(SDL_WindowID id)
        {
#nullable enable
            internal bool TryGetWindow([NotNullWhen(true)] out Window? window) => Window.AllWindows.TryGetValue(new((uint)(id)), out window);

            internal Window? RelativeWindow
            {
                get
                {
                    if (TryGetWindow(id, out var window)) return window;
                    else return null;
                }
            }
        }
    }
}
