using Cider.Attributes;
using Cider.Data;
using Cider.Input;
using Cider.Render;
using System;
using System.Diagnostics;

namespace Cider.Components
{
    /// <summary>
    /// <para>所有组件的基类</para>
    /// </summary>
    public class Component
    {
        /// <summary>
        /// 组件的名称，没有实际作用，传入null会抛出NullReferenceException，默认为空字符串
        /// </summary>
        public string Name { get; set => field = value ?? throw new NullReferenceException(); } = string.Empty;

        /// <summary>
        /// <para>组件的父组件，可能为<c>Scene</c>对象或其它组件，也可能为null</para>
        /// <para><c>Scene</c>对象的<c>Parent</c>属性一定为null</para>
        /// </summary>
        // 修改父组件会自动通知场景树的更新与Window的更新，同时还是OnLoaded的触发器
        public Component? Parent
        {
            get;
            internal set
            {
                if (this is Scene) throw new InvalidOperationException("Scene cannot have Parent");
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
                    {
                        OnWindowChangedDispatcher(null, window); // 一种情况是一开始就不在树中CurrentWindow为null，另一种情况是从一个树挪到另一个树下经上述步骤CurrentWindow已经是null了

                        if (Game.IsInitialized) OnLoadedDispatcher(scene3);
                    }
                }
                else if (value?.Root is Scene scene4)
                {
                    OnAttachToSceneDispatcher(scene4);

                    if (scene4.Window is Window window)
                    {
                        OnWindowChangedDispatcher(null, window);

                        if (Game.IsInitialized) OnLoadedDispatcher(scene4);
                    }
                }

                OnGlobalTransformChangedDispatcher(value?.CreateGlobalTransformArgsFromCurrent() ?? EventArgs.Empty);
            }
        }

        /// <summary>
        /// <para>组件的根场景，可能为null</para>
        /// <para><c>Scene</c>的<c>Root</c>为<c>this</c></para>
        /// </summary>
        // 在OnAttachToSceneDispatcher和OnDetachFromSceneDispatcher中自动更新
        public Scene? Root { get; private set; }

        /// <summary>
        /// 指向<c>Root</c>的<c>Window</c>，当不在组件树或根场景不在窗口下时为<c>null</c>
        /// </summary>
        public Window? CurrentWindow => Root?.Window;

        public Component()
        {
            Children = new(this);
            if (this is Scene scene) Root = scene;
        }

        /// <summary>
        /// 组件是否可见，不可见的组件会跳过<c>OnRender</c>的调用
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// <para>表示子组件的集合</para>
        /// <para>通过这个属性修改父子关系时会自动同步<c>Parent</c>属性</para>
        /// <para>在生命周期回调中子组件是禁止修改的，应使用</para>
        /// </summary>
        public ComponentCollection Children { get; }

        [Dispatcher]
        internal void OnAttachToSceneDispatcher(Scene root)
        {
            Root = root;
            OnAttachToSceneInternal(root);
            OnAttachToScene(root);
            foreach (var item in Children)
                item.OnAttachToSceneDispatcher(root);
        }

        private protected virtual void OnAttachToSceneInternal(Scene root)
        { }

        /// <summary>
        /// <para>当连接到根场景时调用的生命周期函数，可使用<paramref name="root"/>参数或<c>this.Root</c>获取根场景</para>
        /// <para>此函数的方法体是空的，可以省略<c>base</c>调用</para>
        /// </summary>
        /// <param name="root">根场景，在此函数中等价于<c>Root</c>属性</param>
        protected virtual void OnAttachToScene(Scene root)
        { }

        [Dispatcher]
        internal void OnLoadedDispatcher(Scene root)
        {
            Debug.Assert(CurrentWindow is not null);

            OnLoaded(root);
            foreach (var item in Children)
                item.OnLoadedDispatcher(root);
        }

        /// <summary>
        /// <para>当组件就绪时调用的生命周期函数</para>
        /// <para>在组件每次处于就绪状态时此函数都会被调用，因此在组件实例化时就确定的子组件应该直接在构造函数内添加</para>
        /// <para>此函数的方法体是空的，可以省略<c>base</c>调用</para>
        /// </summary>
        /// <param name="root">根场景</param>
        protected virtual void OnLoaded(Scene root)
        { }

        [Dispatcher]
        internal void OnUpdateDispatcher(TimeContext context)
        {
            Debug.Assert(CurrentWindow is not null);

            OnUpdateInternal(context);
            OnUpdate(context);
            foreach (var item in Children)
                item.OnUpdateDispatcher(context);
        }

        /// <summary>
        /// <para>每帧调用的生命周期函数，比<c>OnRender</c>函数更早调用</para>
        /// <para>此函数的方法体是空的，可以省略<c>base</c>调用</para>
        /// </summary>
        /// <param name="context">时间上下文</param>
        protected virtual void OnUpdate(TimeContext context)
        { }

        private protected virtual void OnUpdateInternal(TimeContext context)
        { }

        [Dispatcher]
        internal void OnFixedUpdateDispatcher(TimeContext context)
        {
            Debug.Assert(CurrentWindow is not null);

            OnFixedUpdateInternal(context);
            OnFixedUpdate(context);
            foreach (var item in Children)
                item.OnFixedUpdateDispatcher(context);
        }

        /// <summary>
        /// <para>每一物理周期调用的生命周期函数</para>
        /// <para>此函数的方法体是空的，可以省略<c>base</c>调用</para>
        /// </summary>
        /// <param name="context">时间上下文</param>
        protected virtual void OnFixedUpdate(TimeContext context)
        { }

        private protected virtual void OnFixedUpdateInternal(TimeContext context)
        { }

        /// <summary>
        /// <para>每一帧调用的渲染函数</para>
        /// <para>覆写此函数时可通过<c>base.OnRender</c>控制基类的渲染行为</para>
        /// </summary>
        /// <param name="context">渲染上下文</param>
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
        internal void OnDetachFromSceneDispatcher(Scene root)
        {
            foreach (var item in Children)
                item.OnDetachFromSceneDispatcher(root);
            OnDetachFromSceneInternal(root);
            OnDetachFromScene(root);
            Root = null;
        }

        private protected virtual void OnDetachFromSceneInternal(Scene root)
        { }

        /// <summary>
        /// <para>当从根场景断开时调用的生命周期函数，可使用<paramref name="root"/>参数或<c>this.Root</c>获取根场景</para>
        /// <para>此函数的方法体是空的，可以省略<c>base</c>调用</para>
        /// </summary>
        /// <param name="root">根场景，在此函数中等价于<c>Root</c>属性</param>
        protected virtual void OnDetachFromScene(Scene root)
        { }

        /// <summary>
        /// <para>进行命中测试时调用的函数</para>
        /// <para>当此函数被调用时，<c>Root</c>和<c>CurrentWindow</c>一定不为<c>null</c></para>
        /// </summary>
        /// <param name="result">命中测试结果，该类封装了数个快捷进行命中测试的方法，不应该保存这个对象的引用</param>
        /// <returns>返回<c>false</c>表示未命中，<c>true</c>表示成功命中</returns>
        protected virtual bool HitTest(HitTestResult result)
        {
            return false;
        }

        [Dispatcher]
        internal virtual void HitTestDispatcher(HitTestResult result)
        {
            Debug.Assert(CurrentWindow is not null);

            if (!IsVisible) return;

            var toBeRestored = result.CurrentTransform2D;

            if (HitTest(result)) result.SetComponent(this);
            
            foreach (var item in Children)
            {
                item.HitTestDispatcher(result);
                result.CurrentTransform2D = toBeRestored;
            }
        }

        // 以当前组件为准创建变换链参数
        private protected virtual EventArgs CreateGlobalTransformArgsFromCurrent() => EventArgs.Empty;

        private protected virtual void OnGlobalTransformChangedInternal(EventArgs args)
        {}

        /// <summary>
        /// 通知当前节点与子节点自己的全局变换可能发生改变
        /// </summary>
        /// <param name="args">变换链参数，传入<c>EventArgs</c>实例会导致变换链中断并重新计算</param>
        [Dispatcher]
        internal virtual void OnGlobalTransformChangedDispatcher(EventArgs args)
        {
            OnGlobalTransformChangedInternal(args);
            foreach (var item in Children)
            {
                item.OnGlobalTransformChangedDispatcher(args);
            }
        }

        private protected virtual void OnWindowChangedInternal(Window? oldWindow, Window? newWindow)
        {}

        /// <summary>
        /// <para>当<c>CurrentWindow</c>变更时调用的生命周期函数</para>
        /// <para>在这个函数中<c>CurrentWindow</c>指向<paramref name="oldWindow"/>还是<paramref name="newWindow"/>是不确定的，不应访问<c>CurrentWindow</c>属性</para>
        /// <para>此函数的方法体是空的，可以省略<c>base</c>调用</para>
        /// </summary>
        /// <param name="oldWindow">旧窗口，可能为null</param>
        /// <param name="newWindow">新窗口，可能为null</param>
        protected virtual void OnWindowChanged(Window? oldWindow, Window? newWindow)
        {}

        [Dispatcher]
        internal void OnWindowChangedDispatcher(Window? oldWindow, Window? newWindow)
        {
            OnWindowChangedInternal(oldWindow, newWindow);
            OnWindowChanged(oldWindow, newWindow);

            foreach (var item in Children)
            {
                item.OnWindowChangedDispatcher(oldWindow, newWindow);
            }
        }

        /// <summary>
        /// <para>获取逆序迭代器，从<c>this</c>开始沿<c>Parent</c>属性遍历直到<c>Parent</c>为<c>null</c></para>
        /// <para>例如：</para>
        /// <code>
        /// foreach (var item in component.EnumerateToRoot()
        /// {
        ///     // 其他代码
        /// }
        /// </code>
        /// </summary>
        /// <returns>逆序迭代器</returns>
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

        /// <summary>
        /// <para>等待当前帧的帧末</para>
        /// <para>在生命周期函数中修改Children属性可通过此方法在帧末修改</para>
        /// <para>此方法的返回值必须使用await异步等待，禁止同步等待</para>
        /// <para>通过此方法进行的等待在恢复执行时默认在游戏主循环的线程上执行</para>
        /// <para>已经通过此方法到达帧末时再次调用此方法进行等待会使后续代码同步执行，不会等待到下一帧</para>
        /// <para>例如：</para>
        /// <code>
        /// protected override async void OnLoaded(Scene root)
        /// {
        ///     await WaitForEndOfFrame();
        ///     // 此处为当前帧末
        ///     await WaitForEndOfFrame();
        ///     // 此处的代码会同步执行，不会等待到下一帧
        /// }
        /// </code>
        /// <para>也可通过本地函数使用此方法，例如：</para>
        /// <code>
        /// protected override void OnLoaded(Scene root)
        /// {
        ///     var foo = Foo(); // 可跟踪此Task的状态
        ///     async Task Foo()
        ///     {
        ///         await WaitForEndOfFrame();
        ///         // 后续代码
        ///     }
        /// }
        /// </code>
        /// </summary>
        /// <returns>帧末可等待对象</returns>
        /// <exception cref="InvalidOperationException">当此组件没有连接到Window时抛出</exception>
        public Game.EndOfFrameAwaitable WaitForEndOfFrame() => CurrentWindow is null ? throw new InvalidOperationException("This component is not connected to a window") : new();
    }
}
