using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Collections
{
    public class ChangeQueueableList<T> : List<T>
    {
        private readonly List<T> toAdd = new();
        private readonly List<(T item, Action onRemove)> toRemove = new();

        public void EnqueueAdd(T item) => toAdd.Add(item);

        public void EnqueueRemove(T item, Action onRemove) => toRemove.Add((item, onRemove));

        public void FlushAdd()
        {
            foreach (var item in toAdd)
            {
                Add(item);
            }

            toAdd.Clear();
        }

        public void FlushRemove()
        {
            foreach (var item in toRemove)
            {
                Remove(item.item);
                item.onRemove.Invoke();
            }

            toRemove.Clear();
        }
    }
}
