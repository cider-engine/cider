using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace Cider.Extensions
{
    internal static class MarshalExtensions
    {
        extension(string value)
        {
            public unsafe UnmanagedUtf8 ToUnmanagedUtf8()
            {
                if (value is null) throw new NullReferenceException();

                return new()
                {
                    Pointer = Utf8StringMarshaller.ConvertToUnmanaged(value)
                };
            }
        }

        extension(ReadOnlySpan<char> value)
        {
            public unsafe UnmanagedUtf8 ToUnmanagedUtf8()
            {
                var length = Encoding.UTF8.GetByteCount(value) + 1;
                var ptr = (byte*)NativeMemory.Alloc((nuint)length);
                var span = new Span<byte>(ptr, length);
                Encoding.UTF8.GetBytes(value, span);
                span[^1] = 0;
                return new()
                {
                    Pointer = ptr
                };
            }
        }
    }

    public ref struct UnmanagedUtf8
    {
        public unsafe byte* Pointer;

        public unsafe void Dispose()
        {
            if (Pointer == null) return;
            Utf8StringMarshaller.Free(Pointer);
            Pointer = null;
        }
    }
}
