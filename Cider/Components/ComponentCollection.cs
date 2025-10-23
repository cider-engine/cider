using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Cider.Components
{
    public class ComponentCollection : Collection<Component>
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
            foreach (var item in components)
            {
                item.Parent = Owner;
            }
            _list.AddRange(components);
        }

        protected override void InsertItem(int index, Component item)
        {
            item.Parent = Owner;
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, Component item)
        {
            _list[index].Parent = null;
            item.Parent = Owner;
            base.SetItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            _list[index].Parent = null;
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            foreach (var item in _list)
            {
                item.Parent = null;
            }
            base.ClearItems();
        }
    }
}
