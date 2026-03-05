using Cider.Extensions;
using SDL;
using System;
using System.Runtime.InteropServices.Marshalling;

namespace Cider.Internals
{
    public class SDLProperties : IDisposable
    {
        protected readonly SDL_PropertiesID id;
        protected bool disposedValue;

        public SDLProperties()
        {
            id = SDL3.SDL_CreateProperties();
        }

        internal SDL_PropertiesID Pointer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return id;
            }
        }

        protected unsafe bool GetBooleanProperty(ReadOnlySpan<byte> name)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            fixed (byte* ptr = name)
            {
                return SDL3.SDL_GetBooleanProperty(id, ptr, default(bool));
            }
        }

        protected unsafe float GetFloatProperty(ReadOnlySpan<byte> name)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            fixed (byte* ptr = name)
            {
                return SDL3.SDL_GetFloatProperty(id, ptr, default);
            }
        }

        protected unsafe long GetNumberProperty(ReadOnlySpan<byte> name)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            fixed (byte* ptr = name)
            {
                return SDL3.SDL_GetNumberProperty(id, ptr, default);
            }
        }

        protected unsafe nint GetPointerProperty(ReadOnlySpan<byte> name)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            fixed (byte* ptr = name)
            {
                return SDL3.SDL_GetPointerProperty(id, ptr, default);
            }
        }
#nullable enable
        protected unsafe string? GetStringProperty(ReadOnlySpan<byte> name)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            fixed (byte* ptr = name)
            {
                return Utf8StringMarshaller.ConvertToManaged(SDL3.Unsafe_SDL_GetStringProperty(id, ptr, null));
            }
        }
#nullable restore
        protected unsafe void SetBooleanProperty(ReadOnlySpan<byte> name, bool value)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            fixed (byte* ptr = name)
            {
                SDL3.SDL_SetBooleanProperty(id, ptr, value);
            }
        }

        protected unsafe void SetFloatProperty(ReadOnlySpan<byte> name, float value)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            fixed (byte* ptr = name)
            {
                SDL3.SDL_SetFloatProperty(id, ptr, value);
            }
        }

        protected unsafe void SetNumberProperty(ReadOnlySpan<byte> name, long value)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            fixed (byte* ptr = name)
            {
                SDL3.SDL_SetNumberProperty(id, ptr, value);
            }
        }

        protected unsafe void SetPointerProperty(ReadOnlySpan<byte> name, nint value)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            fixed (byte* ptr = name)
            {
                SDL3.SDL_SetPointerProperty(id, ptr, value);
            }
        }
#nullable enable
        protected unsafe void SetStringProperty(ReadOnlySpan<byte> name, string? value)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            fixed (byte* ptr = name)
            {
                if (value is null) SDL3.SDL_SetStringProperty(id, ptr, null);
                else
                {
                    using var unmanaged = value.ToUnmanagedUtf8();
                    SDL3.SDL_SetStringProperty(id, ptr, unmanaged.Pointer);
                }
            }
        }
#nullable restore
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                SDL3.SDL_DestroyProperties(id);
                disposedValue = true;
            }
        }

        ~SDLProperties()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
