using Cider.Internals;
using SDL;
using System;
using System.Collections.Generic;

namespace Cider
{
    public readonly record struct DisplayId
    {
        public DisplayId(uint id)
        {
            Id = id;
        }
        public readonly uint Id;
        public readonly bool IsInvalid => Id == 0;

        public readonly string Name
        {
            get
            {
                SDLHelpers.EnsureOnMainThread();
                return SDLHelpers.ThrowIfNull(SDL3.SDL_GetDisplayName((SDL_DisplayID)Id));
            }
        }
    }

    public static class Display
    {
        public static unsafe DisplayId GetPrimaryDisplay()
        {
            SDLHelpers.EnsureOnMainThread();

            return new(SDLHelpers.ThrowIfZero((uint)SDL3.SDL_GetPrimaryDisplay()));
        }

        public static unsafe DisplayId[] GetDisplays()
        {
            SDLHelpers.EnsureOnMainThread();

            int count;

            var ptr = SDLHelpers.ThrowIfPtrIsNull(SDL3.SDL_GetDisplays(&count)); // 抛出异常不释放

            var array = new DisplayId[count]; // 显示器数量总不能撑爆内存吧

            // re-interpret
            new Span<DisplayId>(ptr, count).CopyTo(array); // 理论上不抛出异常

            SDL3.SDL_free(ptr); // 懒得写try finally了

            return array;
        }
    }
}
