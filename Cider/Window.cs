using Cider.Collections;
using Cider.Components;
using Cider.Extensions;
using Cider.Internals;
using Cider.Render;
using SDL;
using System;
using System.Collections.Generic;
using System.Drawing;
using static SDL.SDL3;

#if OVERRIDE_WASM
using SpecificWindowFlags = ulong;
#else
using SpecificWindowFlags = SDL.SDL_WindowFlags;
#endif

namespace Cider
{
    public readonly record struct WindowId(uint Id);

    public class WindowCloseRequestedEventArgs : EventArgs
    {
        public bool Handled { get; set; }
    }

    public class WindowEventArgs : EventArgs
    {
        public ulong Timestamp { get; init; }
        public Window Window { get; init; }
    }

    public class Window : IDisposable
    {
        public static ChangeQueueableDictionary<WindowId, Window> AllWindows { get; } = new(EqualityComparer<WindowId>.Create((a, b) => a.Id == b.Id, x => x.Id.GetHashCode()));

        private bool disposedValue;
        private readonly unsafe SDL_Window* _window;

        public WindowId WindowId
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return field;
            }
        }

        public Renderer Renderer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return field;
            }
            private set;
        }

        public bool IsClosed => disposedValue;

        public Scene Scene
        {
            get;
            set
            {
                field.Window = null;
                field = value ?? throw new NullReferenceException();
                value.Window = this;
                if (Game.IsInitialized)
                {
                    value.OnGlobalTransformChangedDispatcher(EventArgs.Empty);
                    value.OnLoadedDispatcher(value); // 如果游戏没有初始化，则在Initialize里调用
                }
            }
        } = new();

        public Point Position
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                SDLHelpers.EnsureOnMainThread();
                int x, y;
                unsafe
                {
                    SDLHelpers.ThrowIfFalse(SDL_GetWindowPosition(_window, &x, &y));
                }
                return new(x, y);
            }
            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                SDLHelpers.EnsureOnMainThread();
                unsafe
                {
                    SDLHelpers.ThrowIfFalse(SDL_SetWindowPosition(_window, value.X, value.Y));
                }
            }
        }

        public Surface Icon
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return field;
            }
            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                SDLHelpers.EnsureOnMainThread();
                unsafe
                {
                    SDLHelpers.ThrowIfFalse(SDL_SetWindowIcon(_window, value.Pointer));
                }
                field = value;
            }
        }

        internal unsafe SDL_Window* Pointer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return _window;
            }
        }

#if OVERRIDE_WASM
        [global::System.Runtime.InteropServices.DllImport("SDL3", CallingConvention = global::System.Runtime.InteropServices.CallingConvention.Cdecl, ExactSpelling = true)]
        private unsafe static extern SDL_Window* SDL_CreateWindow(byte* title, int w, int h, ulong flags);
#endif
        private unsafe Window(ReadOnlySpan<byte> zeroEndedUtf8Title, int width, int height, WindowFlags flags)
        {
            if (OperatingSystem.IsBrowser() && AllWindows.Count > 0) throw new PlatformNotSupportedException("browser doesn't support multiple windows.");

            SDLHelpers.EnsureOnMainThread();
            fixed (byte* title = zeroEndedUtf8Title)
                _window = SDLHelpers.ThrowIfPtrIsNull(SDL_CreateWindow(title, width, height, (SpecificWindowFlags)flags));

            WindowId = new((uint)SDL_GetWindowID(_window));

            Renderer = new(_window);

            Scene.Window = this;

            SDLHelpers.ThrowIfFalse(SDL_SetRenderVSync(Renderer.Pointer, 1));

            AllWindows.EnqueueAdd(WindowId, this);
        }

        public unsafe Window(string title, int width, int height, WindowFlags flags)
        {
            if (OperatingSystem.IsBrowser() && AllWindows.Count > 0) throw new PlatformNotSupportedException("browser doesn't support multiple windows.");

            SDLHelpers.EnsureOnMainThread();
            using var unmanaged = title.ToUnmanagedUtf8();
            _window = SDLHelpers.ThrowIfPtrIsNull(SDL_CreateWindow(unmanaged.Pointer, width, height, (SpecificWindowFlags)flags));

            WindowId = new((uint)SDL_GetWindowID(_window));

            Renderer = new(_window);

            Scene.Window = this;

            SDLHelpers.ThrowIfFalse(SDL_SetRenderVSync(Renderer.Pointer, 1));

            AllWindows.EnqueueAdd(WindowId, this);
        }

        public void Show()
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SDLHelpers.EnsureOnMainThread();
            unsafe
            {
                SDLHelpers.ThrowIfFalse(SDL_ShowWindow(_window));
            }
        }

        public void Hide()
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SDLHelpers.EnsureOnMainThread();
            unsafe
            {
                SDLHelpers.ThrowIfFalse(SDL_HideWindow(_window));
            }
        }

        public void ForceClose()
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SDLHelpers.EnsureOnMainThread();
            ((IDisposable)this).Dispose();
        }

        public void Maximize()
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SDLHelpers.EnsureOnMainThread();
            unsafe
            {
                SDLHelpers.ThrowIfFalse(SDL_MaximizeWindow(_window));
            }
        }

        public void Minimize()
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SDLHelpers.EnsureOnMainThread();
            unsafe
            {
                SDLHelpers.ThrowIfFalse(SDL_MinimizeWindow(_window));
            }
        }

        public void Raise()
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SDLHelpers.EnsureOnMainThread();
            unsafe
            {
                SDLHelpers.ThrowIfFalse(SDL_RaiseWindow(_window));
            }
        }

        public void Restore()
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SDLHelpers.EnsureOnMainThread();
            unsafe
            {
                SDLHelpers.ThrowIfFalse(SDL_RestoreWindow(_window));
            }
        }


        public event EventHandler<Window, WindowCloseRequestedEventArgs> CloseRequested;

        public event EventHandler<Window, EventArgs> Shown;

        public event EventHandler<Window, EventArgs> Hidden;

        public event EventHandler<Window, Point> Moved;

        public event EventHandler<Window, Size> Resized;


        internal void OnShown() => Shown?.Invoke(this, EventArgs.Empty);
        internal void OnHidden() => Hidden?.Invoke(this, EventArgs.Empty);
        internal void OnMoved(Point position) => Moved?.Invoke(this, position);
        internal void OnResized(Size size) => Resized?.Invoke(this, size);
        
        public bool TryClose()
        {
            SDLHelpers.EnsureOnMainThread();
            var args = new WindowCloseRequestedEventArgs();
            CloseRequested?.Invoke(this, args);
            if (args.Handled) return false;

            ((IDisposable)this).Dispose();
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                // 手动Dispose时推入队列，GC回收时直接销毁
                if (disposing)
                {
                    AllWindows.EnqueueRemove(WindowId, () =>
                    {
                        Renderer.Dispose();
                        Renderer = null;
                        unsafe
                        {
                            SDL_DestroyWindow(_window);
                        }
                    });
                }

                else unsafe
                {
                    SDL_DestroyWindow(_window);
                }

                disposedValue = true;
            }
        }

        ~Window()
        {
            Dispose(disposing: false);
        }

        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    [Flags]
    public enum WindowFlags : ulong
    {
        /// <summary>
        /// 窗口处于全屏模式
        /// </summary>
        Fullscreen = 0b1,

        /// <summary>
        /// 窗口可与 OpenGL 上下文一起使用
        /// </summary>
        OpenGL = 0b10,

        /// <summary>
        /// 窗口被遮挡
        /// </summary>
        Occluded = 0b100,

        /// <summary>
        /// 窗口既未映射到桌面上，也未显示在任务栏/停靠栏/窗口列表中；需要调用 SDL_ShowWindow() 才能使其可见
        /// </summary>
        Hidden = 0b1000,

        /// <summary>
        /// 无窗口装饰
        /// </summary>
        Borderless = 0b1_0000,

        /// <summary>
        /// 窗口可以调整大小
        /// </summary>
        Resizable = 0b10_0000,

        /// <summary>
        /// 窗口已最小化
        /// </summary>
        Minimized = 0b100_0000,

        /// <summary>
        /// 窗口已最大化
        /// </summary>
        Maximized = 0b1000_0000,

        /// <summary>
        /// 窗口已捕获鼠标输入
        /// </summary>
        MouseGrabbed = 0b1_0000_0000,

        /// <summary>
        /// 窗口具有输入焦点
        /// </summary>
        InputFocus = 0b10_0000_0000,

        /// <summary>
        /// 窗口具有鼠标焦点
        /// </summary>
        MouseFocus = 0b100_0000_0000,

        /// <summary>
        /// 窗口不是由 SDL 创建的
        /// </summary>
        External = 0b1000_0000_0000,

        /// <summary>
        /// 窗口是模态的
        /// </summary>
        Modal = 0b1_0000_0000_0000,

        /// <summary>
        /// 如果可能，窗口使用高像素密度后备缓冲区
        /// </summary>
        HighPixelDensity = 0b10_0000_0000_0000,

        /// <summary>
        /// 窗口已捕获鼠标（与 MouseGrabbed 无关）
        /// </summary>
        MouseCapture = 0b100_0000_0000_0000,

        /// <summary>
        /// 窗口已启用相对模式
        /// </summary>
        MouseRelativeMode = 0b1000_0000_0000_0000,

        /// <summary>
        /// 窗口应始终位于其他窗口之上
        /// </summary>
        AlwaysOnTop = 0b1_0000_0000_0000_0000,

        /// <summary>
        /// 窗口应被视为utility窗口，不显示在任务栏和窗口列表中
        /// </summary>
        Utility = 0b10_0000_0000_0000_0000,

        /// <summary>
        /// 窗口应被视为tooltip，并且不会获得鼠标或键盘焦点，需要一个父窗口
        /// </summary>
        Tooltip = 0b100_0000_0000_0000_0000,

        /// <summary>
        /// 窗口应被视为弹出菜单，需要一个父窗口
        /// </summary>
        PopupMenu = 0b1000_0000_0000_0000_0000,

        /// <summary>
        /// 窗口已捕获键盘输入
        /// </summary>
        KeyboardGrabbed = 0b1_0000_0000_0000_0000_0000,

        /// <summary>
        /// 窗口处于填充文档模式（仅限 Emscripten），自 SDL 3.4.0 起
        /// </summary>
        FillDocument = 0b10_0000_0000_0000_0000_0000,

        /// <summary>
        /// 窗口可用于 Vulkan Surface
        /// </summary>
        Vulkan = 0b1_0000_0000_0000_0000_0000_0000_0000,

        /// <summary>
        /// 窗口可用于 Metal View
        /// </summary>
        Metal = 0b10_0000_0000_0000_0000_0000_0000_0000,

        /// <summary>
        /// 具有透明缓冲区的窗口
        /// </summary>
        Transparent = 0b100_0000_0000_0000_0000_0000_0000_0000,

        /// <summary>
        /// 窗口不应可获得焦点
        /// </summary>
        NotFocusable = 0b1000_0000_0000_0000_0000_0000_0000_0000,
    }
}
