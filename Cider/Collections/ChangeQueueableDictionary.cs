using System;
using System.Collections.Generic;

namespace Cider.Collections
{
    public class ChangeQueueableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> toAdd = new();
        private readonly List<(TKey key, Action onRemove)> toRemove = new();

        public ChangeQueueableDictionary() : base()
        {}

        public ChangeQueueableDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
        {}

        public void EnqueueAdd(TKey key, TValue value) => toAdd[key] = value;

        public void EnqueueRemove(TKey key, Action onRemove) => toRemove.Add((key, onRemove));

        public void FlushAdd()
        {
            foreach (var item in toAdd)
            {
                this[item.Key] = item.Value;
            }

            toAdd.Clear();
        }

        public void FlushRemove()
        {
            foreach (var item in toRemove)
            {
                if (Remove(item.key))
                {
                    item.onRemove.Invoke();
                }
            }

            toRemove.Clear();
        }
    }
}