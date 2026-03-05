using Cider.Buffers;
using Cider.Extensions;
using Cider.Internals;
using SDL;
using System.Runtime.InteropServices.Marshalling;

namespace Cider
{
    public static class Clipboard
    {
        public static void Clear()
        {
            SDLHelpers.EnsureOnMainThread();

            SDLHelpers.ThrowIfFalse(SDL3.SDL_ClearClipboardData());
        }

        public static unsafe string[] GetMimeTypes()
        {
            SDLHelpers.EnsureOnMainThread();

            nuint typeNum;
            var strings = SDLHelpers.ThrowIfPtrIsNull(SDL3.SDL_GetClipboardMimeTypes(&typeNum));
            var array = new string[typeNum];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = Utf8StringMarshaller.ConvertToManaged(strings[i]);
            }
            SDL3.SDL_free(strings);
            return array;
        }

        public static bool IsTextAvailable()
        {
            SDLHelpers.EnsureOnMainThread();

            return SDL3.SDL_HasClipboardText();
        }

        public static unsafe bool IsDataAvailable(string mimeType)
        {
            SDLHelpers.EnsureOnMainThread();

            using var unmanaged = mimeType.ToUnmanagedUtf8();
            return SDL3.SDL_HasClipboardData(unmanaged.Pointer);
        }
#nullable enable
        public static unsafe ISpanOwner<byte>? GetData(string mimeType)
        {
            SDLHelpers.EnsureOnMainThread();

            using var unmanaged = mimeType.ToUnmanagedUtf8();

            if (!SDL3.SDL_HasClipboardData(unmanaged.Pointer)) return null;

            nuint size;
            var bytes = SDLHelpers.ThrowIfPtrIsNull((byte*)SDL3.SDL_GetClipboardData(unmanaged.Pointer, &size));
            return new SDLMemoryOwner(bytes, size);
        }

        public static string? GetText()
        {
            SDLHelpers.EnsureOnMainThread();

            if (SDL3.SDL_HasClipboardText())
            {
                return SDL3.SDL_GetClipboardText()!;
            }

            else
            {
                return null;
            }
        }

        public static void SetText(string? text)
        {
            SDLHelpers.EnsureOnMainThread();

            if (text is null)
            {
                SDLHelpers.ThrowIfFalse(SDL3.SDL_ClearClipboardData());
                return;
            }

            using var unmananged = text.ToUnmanagedUtf8();
            unsafe
            {
                SDLHelpers.ThrowIfFalse(SDL3.SDL_SetClipboardText(unmananged.Pointer));
            }
        }
#nullable restore
    }
}
