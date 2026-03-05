using Cider.Attributes;
using Cider.Data;
using Cider.Input;
using Cider.Render;
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
                if (field is Scene scene1) OnDetachFromSceneDispatcher(scene1);
                else if (Root is Scene scene2) OnDetachFromSceneDispatcher(scene2);

                if (value is Scene scene3) OnAttachToSceneDispatcher(scene3);
                else if (value?.Root is Scene scene4) OnAttachToSceneDispatcher(scene4);

                field = value;
            }
        }

        public Scene? Root { get; private set; }

        public Window? CurrentWindow => Root?.Window;
#nullable disable

        public Component()
        {
            Children = new(this);
        }

        public bool IsVisible { get; set; } = true;

        public ComponentCollection Children { get; }

        [Dispatcher]
        internal virtual void OnAttachToSceneDispatcher(Scene root)
        {
            Root = root;
            OnAttachToScene(root);
            foreach (var item in Children)
                item.OnAttachToSceneDispatcher(root);
        }

        protected virtual void OnAttachToScene(Scene root)
        { }

        [Dispatcher]
        internal virtual void OnLoadedDispatcher(Scene root)
        {
            OnLoaded(root);
            foreach (var item in Children)
                item.OnLoadedDispatcher(root);
        }

        protected virtual void OnLoaded(Scene root)
        { }

        [Dispatcher]
        internal virtual void OnUpdateDispatcher(TimeContext context)
        {
            OnUpdate(context);
            foreach (var item in Children)
                item.OnUpdateDispatcher(context);
        }

        protected virtual void OnUpdate(TimeContext context)
        { }

        [Dispatcher]
        internal virtual void OnFixedUpdateDispatcher(TimeContext context)
        {
            OnFixedUpdate(context);
            foreach (var item in Children)
                item.OnFixedUpdateDispatcher(context);
        }

        protected virtual void OnFixedUpdate(TimeContext context)
        { }

        protected virtual void OnRender(RenderContext context)
        {}

        [Dispatcher]
        internal virtual void OnRenderDispatcher(RenderContext context)
        {
            if (!IsVisible) return;
            OnRender(context);
            foreach (var item in Children)
                item.OnRenderDispatcher(context);
        }

        [Dispatcher]
        internal virtual void OnDetachFromSceneDispatcher(Scene root)
        {
            foreach (var item in Children)
                item.OnDetachFromSceneDispatcher(root);
            OnDetachFromScene(root);
            Root = null;
        }

        protected virtual void OnDetachFromScene(Scene root)
        { }

        protected virtual bool HitTest(HitTestResult result)
        {
            return false;
        }

        [Dispatcher]
        internal virtual void HitTestDispatcher(HitTestResult result)
        {
            if (!IsVisible) return;

            var toBeRestored = result.CurrentTransform2D;

            if (HitTest(result)) result.SetComponent(this);
            
            foreach (var item in Children)
            {
                item.HitTestDispatcher(result);
                result.CurrentTransform2D = toBeRestored;
            }
        }

        protected virtual void OnGlobalTransformChanged(EventArgs args)
        {}

        [Dispatcher]
        internal virtual void OnGlobalTransformChangedDispatcher(EventArgs args)
        {
            OnGlobalTransformChanged(args);
            foreach (var item in Children)
            {
                item.OnGlobalTransformChangedDispatcher(args);
            }
        }
#nullable enable
        protected virtual void OnWindowChanged(Window? oldWindow, Window? newWindow)
        {}

        [Dispatcher]
        internal void OnWindowChangedDispatcher(Window? oldWindow, Window? newWindow)
        {
            OnWindowChanged(oldWindow, newWindow);

            foreach (var item in Children)
            {
                item.OnWindowChangedDispatcher(oldWindow, newWindow);
            }
        }

        public ToRootEnumerator EnumerateToRoot() => new(this);

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

            public readonly ToRootEnumerator GetEnumerator() => this;
        }
    }
}
