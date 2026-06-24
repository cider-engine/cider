using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Cider.Buffers
{
    public unsafe class UnmanagedMemoryOwner : IDroppableSpanOwner<byte>
    {
        private readonly byte* ptr;
        private readonly nuint length;
        private bool disposedValue;

        public UnmanagedMemoryOwner(nuint length)
        {
            ptr = (byte*)NativeMemory.Alloc(length);
            this.length = length;
        }

        public Span<byte> Span
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return new(ptr, checked((int)length));
            }
        }

        public void DropOwnership(out Span<byte> span, out delegate* unmanaged[Cdecl]<void*, void*> getCallback, out delegate* unmanaged[Cdecl]<void*, void> freeCallback)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            span = Span;
            getCallback = &Get;
            freeCallback = &Free;
            GC.SuppressFinalize(this);
            disposedValue = true;

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static void* Get(void* ptr)
            {
                return ptr;
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static void Free(void* ptr)
            {
                NativeMemory.Free(ptr);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                //if (disposing)
                //{
                //}

                NativeMemory.Free(ptr);
                GetPointer(this) = null;
                disposedValue = true;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(ptr))]
            static extern ref byte* GetPointer(UnmanagedMemoryOwner @this);
        }

        ~UnmanagedMemoryOwner()
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
