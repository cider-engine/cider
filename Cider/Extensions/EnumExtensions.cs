using SDL;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Cider.Extensions
{
    internal static class EnumExtensions
    {
        extension(SDL_WindowID id)
        {
#nullable enable
            public bool TryGetWindow([NotNullWhen(true)] out Window? window) => Window.AllWindows.TryGetValue(new((uint)(id)), out window);

            public Window? RelativeWindow
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
