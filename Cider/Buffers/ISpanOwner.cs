using System;

namespace Cider.Buffers
{
    public interface ISpanOwner<T> : IDisposable
    {
        Span<T> Span { get; }
    }
}
