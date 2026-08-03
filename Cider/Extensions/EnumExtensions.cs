using SDL;
using System;

namespace Cider.Extensions
{
    internal static class EnumExtensions
    {
        extension(SDL_WindowID id)
        {
            public Window? RelativeWindow => Window.GetWindowFromId(new((uint)id));
        }
    }
}
