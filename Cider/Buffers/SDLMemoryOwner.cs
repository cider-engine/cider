using SDL;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Cider.Buffers
{
    internal unsafe class SDLMemoryOwner : ISpanOwner<byte>
    {
        private readonly byte* ptr;
        private readonly nuint length;
        private bool disposedValue;

        public SDLMemoryOwner(byte* ptr, nuint length)
        {
            this.ptr = ptr;
            this.length = length;
        }

        public Span<byte> Span
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return new(ptr, (int)length);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                //if (disposing)
                //{
                //}

                SDL3.SDL_free(ptr);
                GetPointer(this) = null;
                disposedValue = true;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(ptr))]
            static extern ref byte* GetPointer(SDLMemoryOwner @this);
        }

        ~SDLMemoryOwner()
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
