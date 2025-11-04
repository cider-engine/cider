using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Converters
{
    public readonly ref struct CollectionValueConverter<T>
    {
        private readonly ReadOnlySpan<T> _values;

        public CollectionValueConverter(ReadOnlySpan<T> values)
        {
            _values = values;
        }

        public static implicit operator T(in CollectionValueConverter<T> converter)
        {
            if (converter._values.Length != 1) 
                throw new InvalidOperationException("Cannot convert to single value when collection doesn't have one element.");

            return converter._values[0];
        }

        public static implicit operator T[](in CollectionValueConverter<T> converter)
        {
            var array = new T[converter._values.Length];
            converter._values.CopyTo(array);
            return array;
        }

        public static implicit operator List<T>(in CollectionValueConverter<T> converter)
        {
            var list = new List<T>(converter._values.Length);
            foreach (var item in converter._values)
            {
                list.Add(item);
            }
            return list;
        }
    }
}
