using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Cider.Components
{
    public class ComponentCollection : Collection<Component>, IEnumerable<Component>
    {
        private readonly List<Component> _list; // 基类的IList为private，这里持有该list引用

        public Component Owner { get; }

        public ComponentCollection(Component owner) : this(new(), owner)
        {
        }

        public ComponentCollection(List<Component> list, Component owner) : base(list)
        {
            _list = list;
            Owner = owner;
        }

        public new List<Component>.Enumerator GetEnumerator() => _list.GetEnumerator();

        public void AddRange(params ReadOnlySpan<Component> components)
        {
            // 先设置 Parent，再把项加入集合，最后统一触发事件，确保事件处理器能看到集合已包含这些项
            foreach (var item in components)
            {
                item.Parent = Owner;
            }

            _list.AddRange(components);

            foreach (var item in components)
            {
                ComponentAdded?.Invoke(Owner, item);
            }
        }

        protected override void InsertItem(int index, Component item)
        {
            // 先插入，再设置 Parent 并触发事件，保证事件处理器看到项已经在集合内
            base.InsertItem(index, item);
            item.Parent = Owner;
            ComponentAdded?.Invoke(Owner, item);
        }

        protected override void SetItem(int index, Component item)
        {
            // 保存旧项，替换集合内容后再调整 Parent 并触发事件
            var old = _list[index];
            base.SetItem(index, item);

            old.Parent = null;
            item.Parent = Owner;

            ComponentRemoved?.Invoke(Owner, old);
            ComponentAdded?.Invoke(Owner,item);
        }

        protected override void RemoveItem(int index)
        {
            // 先取出将被移除的项，执行移除，再触发事件并清除 Parent
            var removed = _list[index];
            base.RemoveItem(index);

            removed.Parent = null;
            ComponentRemoved?.Invoke(Owner, removed);
        }

        protected override void ClearItems()
        {
            // 复制旧集合，执行清空，再对每个旧项触发移除事件并清空 Parent
            var oldItems = _list.ToArray();
            base.ClearItems();

            foreach (var item in oldItems)
            {
                item.Parent = null;
                ComponentRemoved?.Invoke(Owner, item);
            }
        }

        public delegate void ComponentChangedEventHandler(Component owner, Component changedComponent);

        public event ComponentChangedEventHandler ComponentAdded;
        public event ComponentChangedEventHandler ComponentRemoved;
    }
}
