using Cider.Components;
using System;
using System.Collections.Generic;

namespace Cider.Extensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static class CollectionExtensions
    {
        extension<T>(T[] array)
        {
            public void Add(ReadOnlySpan<T> items)
            {
                if (items.Length > array.Length) throw new ArgumentOutOfRangeException(nameof(items));
                items.CopyTo(array);
            }
        }

        extension<T>(ICollection<T> collection)
        {
            public void Add(ReadOnlySpan<T> items)
            {
                foreach (var item in items) collection.Add(item);
            }
        }

        extension<T>(IList<T> list)
        {
            public void Add(ReadOnlySpan<T> items)
            {
                foreach (var item in items) list.Add(item);
            }
        }

        extension<T>(List<T> list)
        {
            public void Add(ReadOnlySpan<T> items)
            {
                list.Capacity = list.Count + items.Length;
                list.AddRange(items);
            }
        }

        extension(ComponentCollection components)
        {
            public void Add(ReadOnlySpan<Component> items)
            {
                components.Capacity = components.Count + items.Length;
                components.AddRange(items);
            }
        }
    }
}
