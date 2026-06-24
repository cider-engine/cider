using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Cider.Collections
{
    public struct CollectionCopyEnumerator<T> : IEnumerator<T>
    {
        private T[] _rentedArray;
        private readonly int _count;
        private int _index;
        private T _current;

        public CollectionCopyEnumerator(ICollection<T> collection)
        {
            _count = collection.Count;
            _rentedArray = ArrayPool<T>.Shared.Rent(_count);
            collection.CopyTo(_rentedArray, 0);
            _index = -1;
            _current = default;
        }

        public readonly T Current => _current!;

        readonly object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            if (++_index < _count)
            {
                _current = _rentedArray[_index];
                return true;
            }

            _current = default;
            return false;
        }

        public void Reset()
        {
            _index = -1;
            _current = default;
        }

        public void Dispose()
        {
            if (_rentedArray != null)
            {
                ArrayPool<T>.Shared.Return(_rentedArray, clearArray: true);
                _rentedArray = null;
            }
        }
    }
}
