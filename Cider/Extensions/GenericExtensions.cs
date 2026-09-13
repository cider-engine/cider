using System;
using System.ComponentModel;
using System.Numerics;

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

        extension<T>(T number) where T : INumber<T>
        {
            public static T Repeat(T t, T length)
            {
                T r = t % length;
                if (r < T.Zero) r += length;
                return T.Clamp(r, T.Zero, length);
            }

            public static T PingPong(T t, T length)
            {
                t = Repeat(t, length * T.CreateChecked(2));
                return length - T.Abs(t - length);
            }
        }
    }
}
