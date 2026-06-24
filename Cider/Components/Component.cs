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
                if (field is Scene scene1)
                {
                    if (scene1.Window is Window window)
                        OnWindowChangedDispatcher(window, null); // 如果已连接到窗口中就通知，与直接修改Window属性的通知不一样

                    OnDetachFromSceneDispatcher(scene1);
                }
                else if (Root is Scene scene2)
                {
                    if (scene2.Window is Window window)
                        OnWindowChangedDispatcher(window, null);

                    OnDetachFromSceneDispatcher(scene2);
                }

                field = value;

                if (value is Scene scene3)
                {
                    OnAttachToSceneDispatcher(scene3);

                    if (scene3.Window is Window window)
                        OnWindowChangedDispatcher(null, window); // 不在树中CurrentWindow为null，从一个树挪到另一个树下经上述步骤CurrentWindow已经是null了

                    if (Game.IsInitialized && scene3.Window is not null) OnLoadedDispatcher(scene3);
                }
                else if (value?.Root is Scene scene4)
                {
                    OnAttachToSceneDispatcher(scene4);

                    if (scene4.Window is Window window)
                        OnWindowChangedDispatcher(null, window);

                    if (Game.IsInitialized && scene4.Window is not null) OnLoadedDispatcher(scene4);
                }
            }
        }

        public Scene? Root { get; private set; }

        public Window? CurrentWindow => Root?.Window;
#nullable disable

        public Component()
        {
            Children = new(this);
            if (this is Scene scene) Root = scene;
        }

        public bool IsVisible { get; set; } = true;

        public ComponentCollection Children { get; }

        [Dispatcher]
        internal void OnAttachToSceneDispatcher(Scene root)
        {
            Root = root;
            OnAttachToScene(root);
            foreach (var item in Children)
                item.OnAttachToSceneDispatcher(root);
        }

        protected virtual void OnAttachToScene(Scene root)
        { }

        [Dispatcher]
        internal void OnLoadedDispatcher(Scene root)
        {
            if (CurrentWindow is null) return;
            OnLoaded(root);
            foreach (var item in Children)
                item.OnLoadedDispatcher(root);
        }

        protected virtual void OnLoaded(Scene root)
        { }

        [Dispatcher]
        internal virtual void OnUpdateDispatcher(TimeContext context)
        {
            if (CurrentWindow is null) return;
            OnUpdate(context);
            foreach (var item in Children)
                item.OnUpdateDispatcher(context);
        }

        protected virtual void OnUpdate(TimeContext context)
        { }

        [Dispatcher]
        internal virtual void OnFixedUpdateDispatcher(TimeContext context)
        {
            if (CurrentWindow is null) return;
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
