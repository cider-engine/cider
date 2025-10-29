using Cider.Data;
using Cider.Input;
using Cider.Render;
using Microsoft.Xna.Framework;
using System;

namespace Cider.Components
{
    public class Component
    {
#nullable enable
        public string Name { get; set => field = value ?? throw new NullReferenceException(); } = string.Empty;

        public Component? Parent
        {
            get;
            internal set
            {
                if (this is Scene) throw new InvalidOperationException();
                if (field is Scene scene1) OnDetachToScene(scene1);
                else if (Root is Scene scene2) OnDetachToScene(scene2);

                if (value is Scene scene3) OnAttachToScene(scene3);
                else if (value?.Root is Scene scene4) OnAttachToScene(scene4);

                field = value;
            }
        }

        public Scene? Root { get; private set; }
#nullable disable

        public Component()
        {
            Children = new(this);
        }

        public bool IsVisible { get; set; } = true;

        public ComponentCollection Children { get; }

        protected internal virtual void OnAttachToScene(Scene root)
        {
            Root = root;
            foreach (var item in Children)
                item.OnAttachToScene(root);
        }

        protected internal virtual void OnLoaded(Scene root)
        {
            foreach (var item in Children)
                item.OnLoaded(root);
        }

        protected internal virtual void OnUpdate(in TimeContext context)
        {
            foreach (var item in Children)
                item.OnUpdate(context);
        }

        protected internal virtual void OnFixedUpdate(in TimeContext context)
        {
            foreach (var item in Children)
                item.OnFixedUpdate(context);
        }

        protected internal virtual void OnDraw(RenderContext context)
        {
            if (!IsVisible) return;
            foreach (var item in Children)
                item.OnDraw(context);
        }

        protected internal virtual void OnDetachToScene(Scene root)
        {
            foreach (var item in Children)
                item.OnDetachToScene(root);
            Root = null;
        }

        protected internal virtual bool HitTest(HitTestResult result)
        {
            return false;
        }


        internal virtual void ForeachHitTest(HitTestResult result)
        {
            if (!IsVisible) return;
            if (HitTest(result)) result.SetComponent(this);
            
            foreach (var item in Children)
                item.ForeachHitTest(result);
        }

        public ToRootEnumeratorGetter GetToRootEnumeratorGetter() => new(this);

#nullable enable
        public struct ToRootEnumeratorGetter(Component? start)
        {
            public readonly ToRootEnumerator GetEnumerator() => new(start);
        }

        public struct ToRootEnumerator(Component? start)
        {
            private bool _started;
            private Component? _current = start;

            public readonly Component? Current => _current;

            public bool MoveNext()
            {
                if (_started)
                {
                    if (_current is null) return false;
                    _current = _current.Parent;
                    return _current is not null;
                }

                _started = true;
                return _current is not null;
            }
        }
    }
}
