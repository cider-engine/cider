using Cider.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Cider.Collections
{
    public class ItemObservableCollection<T> : Collection<T>, IEnumerable<T>
    {
        private readonly List<T> _list; // 基类的IList为private，这里持有该list引用

        public ItemObservableCollection() : this(new())
        {
        }

        public ItemObservableCollection(List<T> list) : base(list)
        {
            _list = list;
        }

        public int Capacity
        {
            get => _list.Capacity;
            set => _list.Capacity = value;
        }

        public void AddRange(params ReadOnlySpan<T> items)
        {
            // 先把项加入集合，再设置 Parent，最后统一触发事件，确保事件处理器能看到集合已包含这些项
            _list.AddRange(items);

            foreach (var item in items)
            {
                ItemAdded?.Invoke(this, item);
            }
        }

        protected override void InsertItem(int index, T item)
        {
            // 先插入，再设置 Parent 并触发事件，保证事件处理器看到项已经在集合内
            base.InsertItem(index, item);
            ItemAdded?.Invoke(this, item);
        }

        protected override void SetItem(int index, T item)
        {
            // 保存旧项，替换集合内容后再调整 Parent 并触发事件
            var old = _list[index];
            base.SetItem(index, item);

            ItemRemoved?.Invoke(this, old);
            ItemAdded?.Invoke(this, item);
        }

        protected override void RemoveItem(int index)
        {
            // 先取出将被移除的项，执行移除，再触发事件并清除 Parent
            var removed = _list[index];
            base.RemoveItem(index);

            ItemRemoved?.Invoke(this, removed);
        }

        protected override void ClearItems()
        {
            // 复制旧集合，执行清空，再对每个旧项触发移除事件并清空 Parent
            var oldItems = _list.ToArray();
            base.ClearItems();

            foreach (var item in oldItems)
            {
                ItemRemoved?.Invoke(this, item);
            }
        }

        public delegate void ItemChangedEventHandler(ItemObservableCollection<T> collection, T changedItem);

        public event ItemChangedEventHandler ItemAdded;
        public event ItemChangedEventHandler ItemRemoved;
    }
}
