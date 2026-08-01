using System;
using System.ComponentModel;

namespace Cider.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GenericExtensions
    {
        extension<T>(T item)
        {
            public Action<T> Invoke
            {
                set => value.Invoke(item);
            }
        }
    }
}
