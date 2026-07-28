using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Extensions
{
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
