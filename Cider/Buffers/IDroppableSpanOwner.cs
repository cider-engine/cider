using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Buffers
{
    public interface IDroppableSpanOwner<T> : ISpanOwner<T>
    {
        unsafe void DropOwnership(out Span<T> span, out delegate* unmanaged[Cdecl]<void*, void*> getCallback, out delegate* unmanaged[Cdecl]<void*, void> freeCallback);
    }
}
