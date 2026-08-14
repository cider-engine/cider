using Cider.Components;
using Cider.Components.In2D;
using Cider.Extensions;
using Cider.Internals;
using Cider.Render;
using SDL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using static SDL.SDL3;

#if true
using SpecificWindowFlags = ulong;
#else
using SpecificWindowFlags = SDL.SDL_WindowFlags;
#endif

namespace Cider
{
    public readonly record struct WindowId(uint Id)
    {
        public readonly bool IsInvalid => Id == 0;
    }

    public class WindowCloseRequestedEventArgs : EventArgs
    {
        public bool Handled { get; set; }
    }

    public class Window : IDisposable
    {
        private static readonly Dictionary<WindowId, Window> _allWindows = new(EqualityComparer<WindowId>.Create((a, b) => a == b, x => x.GetHashCode()));

        public static ICollection<Window> AllWindows => _allWindows.Values;

        public static Window? GetWindowFromId(WindowId id)
        {
            if (_allWindows.TryGetValue(id, out var window)) return window;
            if (id.IsInvalid) return null;
            unsafe
            {
                return new(id, SDL_GetWindowFromID((SDL_WindowID)id.Id), new());
            }
        }

        private bool disposedValue;
        private readonly unsafe SDL_Window* _window;
        private readonly Renderer _renderer;

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
                return _renderer;
            }
        }

        public bool IsClosed => disposedValue;

        public Scene Scene
        {
            get;
            set
            {
                field?.Window = null; // 第一次设置的时候field为null
                field = value ?? throw new NullReferenceException();
                value.Window = this;
                if (Game.IsInitialized)
                {
                    value.OnLoadedDispatcher(value); // 如果游戏没有初始化，则在Initialize里调用
                }
            }
        }

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

        public unsafe Size Size
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                int width, height;
                SDLHelpers.ThrowIfFalse(SDL_GetWindowSize(_window, &width, &height));
                return new(width, height);
            }
            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                SDLHelpers.ThrowIfFalse(SDL_SetWindowSize(_window, value.Width, value.Height));
            }
        }

        public Surface? Icon
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
                    SDLHelpers.ThrowIfFalse(SDL_SetWindowIcon(_window, value!.Pointer));
                }
                field = value;
            }
        }

        public Color BackgroundColor
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return field;
            }
            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                field = value;
            }
        } = Color.Black;

        public Color ClearColor
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return field;
            }
            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                field = value;
            }
        } = Color.Black;

        internal unsafe SDL_Window* Pointer
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return _window;
            }
        }

        [global::System.Runtime.InteropServices.DllImport("SDL3", CallingConvention = global::System.Runtime.InteropServices.CallingConvention.Cdecl, ExactSpelling = true)]
        private unsafe static extern SDL_Window* SDL_CreateWindow(byte* title, int w, int h, SpecificWindowFlags flags);

        public unsafe Window(string title, Scene scene, int width, int height, WindowFlags flags)
        {
            if (OperatingSystem.IsBrowser() && AllWindows.Count > 0) throw new PlatformNotSupportedException("browser doesn't support multiple windows.");

            SDLHelpers.EnsureOnMainThread();
            using var unmanaged = title.ToUnmanagedUtf8();
            _window = SDLHelpers.ThrowIfPtrIsNull(SDL_CreateWindow(unmanaged.Pointer, width, height, (SpecificWindowFlags)flags));

            WindowId = new((uint)SDL_GetWindowID(_window));

            _renderer = new(_window);

            scene.Window = this;

            Scene = scene;

            SDLHelpers.ThrowIfFalse(SDL_SetRenderVSync(Renderer.Pointer, 1)); // 默认强制垂直同步

            _allWindows.Add(WindowId, this);
        }

        private unsafe Window(WindowId id, SDL_Window* window, Scene scene)
        {
            _window = window;

            WindowId = id;

            _renderer = new(_window);

            scene.Window = this;

            Scene = scene;

            SDLHelpers.ThrowIfFalse(SDL_SetRenderVSync(Renderer.Pointer, 1)); // 默认强制垂直同步

            _allWindows.Add(WindowId, this);
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

        public void SetTextInputArea(Rectangle? target, int cursorOffsetToTargetX = 0)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SDLHelpers.EnsureOnMainThread();
            unsafe
            {
                if (target is Rectangle x)
                {
                    SDL_Rect rect = new()
                    {
                        x = x.X,
                        y = x.Y,
                        w = x.Width,
                        h = x.Height
                    };

                    SDLHelpers.ThrowIfFalse(SDL_SetTextInputArea(_window, &rect, cursorOffsetToTargetX));
                }

                else SDLHelpers.ThrowIfFalse(SDL_SetTextInputArea(_window, null, cursorOffsetToTargetX));
            }
        }

        public void StartTextInput(TextInputOptions? options = null)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SDLHelpers.EnsureOnMainThread();
            unsafe
            {
                SDLHelpers.ThrowIfFalse(options is null ? SDL_StartTextInput(_window) : SDL_StartTextInputWithProperties(_window, options.Pointer));
            }
        }

        public void StopTextInput()
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SDLHelpers.EnsureOnMainThread();
            unsafe
            {
                SDLHelpers.ThrowIfFalse(SDL_StopTextInput(_window));
            }
        }

        public void ShowMessageBox(MessageBoxType type, ReadOnlySpan<char> title, ReadOnlySpan<char> message)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SDLHelpers.EnsureOnMainThread();
            unsafe
            {
                using var titlePtr = title.ToUnmanagedUtf8();
                using var messagePtr = message.ToUnmanagedUtf8();
                SDLHelpers.ThrowIfFalse(SDL_ShowSimpleMessageBox((SDL_MessageBoxFlags)type, titlePtr.Pointer, messagePtr.Pointer, _window));
            }
        }


        private readonly WeakReference<Component2D> _focusedComponentRefrence = new(null!);

        internal Component2D? FocusedComponent => _focusedComponentRefrence.TryGetTarget(out var target) ? target : null;

        internal void SetFocus(Component2D? gettingFocusComponent)
        {
            _focusedComponentRefrence.TryGetTarget(out Component2D? losingFocusComponent);

            if (ReferenceEquals(gettingFocusComponent, losingFocusComponent)) return;

            losingFocusComponent?.IsFocused = false;

            gettingFocusComponent?.IsFocused = true;

            losingFocusComponent?.OnLostFocus(losingFocusComponent, gettingFocusComponent);

            gettingFocusComponent?.OnGotFocus(gettingFocusComponent, losingFocusComponent);
        }

        internal void ClearFocus() => SetFocus(null);

#nullable disable
        public event EventHandler<Window, WindowCloseRequestedEventArgs> CloseRequested;

        public event EventHandler<Window, EventArgs> Shown;

        public event EventHandler<Window, EventArgs> Hidden;

        public event EventHandler<Window, Point> Moved;

        public event EventHandler<Window, Size> Resized;
#nullable restore

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
                if (disposing)
                {
                    _allWindows.Remove(WindowId);
                    Renderer.Dispose();
                }

                unsafe
                {
                    SDL_DestroyWindow(_window);
                    GetPointer(this) = null;
                }

                disposedValue = true;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(_window))]
            static extern unsafe ref SDL_Window* GetPointer(Window @this);
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
        FullScreen = SDL_WindowFlags.SDL_WINDOW_FULLSCREEN,

        /// <summary>
        /// 窗口可与 OpenGL 上下文一起使用
        /// </summary>
        OpenGL = SDL_WindowFlags.SDL_WINDOW_OPENGL,

        /// <summary>
        /// 窗口被遮挡
        /// </summary>
        Occluded = SDL_WindowFlags.SDL_WINDOW_OCCLUDED,

        /// <summary>
        /// 窗口既未映射到桌面上，也未显示在任务栏/停靠栏/窗口列表中；需要调用 SDL_ShowWindow() 才能使其可见
        /// </summary>
        Hidden = SDL_WindowFlags.SDL_WINDOW_HIDDEN,

        /// <summary>
        /// 无窗口装饰
        /// </summary>
        Borderless = SDL_WindowFlags.SDL_WINDOW_BORDERLESS,

        /// <summary>
        /// 窗口可以调整大小
        /// </summary>
        Resizable = SDL_WindowFlags.SDL_WINDOW_RESIZABLE,

        /// <summary>
        /// 窗口已最小化
        /// </summary>
        Minimized = SDL_WindowFlags.SDL_WINDOW_MINIMIZED,

        /// <summary>
        /// 窗口已最大化
        /// </summary>
        Maximized = SDL_WindowFlags.SDL_WINDOW_MAXIMIZED,

        /// <summary>
        /// 窗口已捕获鼠标输入
        /// </summary>
        MouseGrabbed = SDL_WindowFlags.SDL_WINDOW_MOUSE_GRABBED,

        /// <summary>
        /// 窗口具有输入焦点
        /// </summary>
        InputFocus = SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS,

        /// <summary>
        /// 窗口具有鼠标焦点
        /// </summary>
        MouseFocus = SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS,

        /// <summary>
        /// 窗口不是由 SDL 创建的
        /// </summary>
        External = SDL_WindowFlags.SDL_WINDOW_EXTERNAL,

        /// <summary>
        /// 窗口是模态的
        /// </summary>
        Modal = SDL_WindowFlags.SDL_WINDOW_MODAL,

        /// <summary>
        /// 如果可能，窗口使用高像素密度后备缓冲区
        /// </summary>
        HighPixelDensity = SDL_WindowFlags.SDL_WINDOW_HIGH_PIXEL_DENSITY,

        /// <summary>
        /// 窗口已捕获鼠标（与 MouseGrabbed 无关）
        /// </summary>
        MouseCapture = SDL_WindowFlags.SDL_WINDOW_MOUSE_CAPTURE,

        /// <summary>
        /// 窗口已启用相对模式
        /// </summary>
        MouseRelativeMode = SDL_WindowFlags.SDL_WINDOW_MOUSE_RELATIVE_MODE,

        /// <summary>
        /// 窗口应始终位于其他窗口之上
        /// </summary>
        AlwaysOnTop = SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP,

        /// <summary>
        /// 窗口应被视为utility窗口，不显示在任务栏和窗口列表中
        /// </summary>
        Utility = SDL_WindowFlags.SDL_WINDOW_UTILITY,

        /// <summary>
        /// 窗口应被视为tooltip，并且不会获得鼠标或键盘焦点，需要一个父窗口
        /// </summary>
        Tooltip = SDL_WindowFlags.SDL_WINDOW_TOOLTIP,

        /// <summary>
        /// 窗口应被视为弹出菜单，需要一个父窗口
        /// </summary>
        PopupMenu = SDL_WindowFlags.SDL_WINDOW_POPUP_MENU,

        /// <summary>
        /// 窗口已捕获键盘输入
        /// </summary>
        KeyboardGrabbed = SDL_WindowFlags.SDL_WINDOW_KEYBOARD_GRABBED,

        /// <summary>
        /// 窗口处于填充文档模式（仅限 Emscripten），自 SDL 3.4.0 起
        /// </summary>
        [SupportedOSPlatform("browser")]
        FillDocument = 0x200000uL,

        /// <summary>
        /// 窗口可用于 Vulkan Surface
        /// </summary>
        Vulkan = SDL_WindowFlags.SDL_WINDOW_VULKAN,

        /// <summary>
        /// 窗口可用于 Metal View
        /// </summary>
        Metal = SDL_WindowFlags.SDL_WINDOW_METAL,

        /// <summary>
        /// 具有透明缓冲区的窗口
        /// </summary>
        Transparent = SDL_WindowFlags.SDL_WINDOW_TRANSPARENT,

        /// <summary>
        /// 窗口不应可获得焦点
        /// </summary>
        NotFocusable = SDL_WindowFlags.SDL_WINDOW_NOT_FOCUSABLE,
    }

    public class TextInputOptions : PropertyBase
    {
        public TextInputType Type
        {
            get => (TextInputType)GetNumberProperty(SDL_PROP_TEXTINPUT_TYPE_NUMBER);
            set => SetNumberProperty(SDL_PROP_TEXTINPUT_TYPE_NUMBER, (long)value);
        }
    }

    public enum TextInputType
    {
        Text = SDL_TextInputType.SDL_TEXTINPUT_TYPE_TEXT,
        TextName = SDL_TextInputType.SDL_TEXTINPUT_TYPE_TEXT_NAME,
        TextEmail = SDL_TextInputType.SDL_TEXTINPUT_TYPE_TEXT_EMAIL,
        TextUsername = SDL_TextInputType.SDL_TEXTINPUT_TYPE_TEXT_USERNAME,
        TextPasswordHidden = SDL_TextInputType.SDL_TEXTINPUT_TYPE_TEXT_PASSWORD_HIDDEN,
        TextPasswordVisible = SDL_TextInputType.SDL_TEXTINPUT_TYPE_TEXT_PASSWORD_VISIBLE,
        Number = SDL_TextInputType.SDL_TEXTINPUT_TYPE_NUMBER,
        NumberPasswordHidden = SDL_TextInputType.SDL_TEXTINPUT_TYPE_NUMBER_PASSWORD_HIDDEN,
        NumberPasswordVisible = SDL_TextInputType.SDL_TEXTINPUT_TYPE_NUMBER_PASSWORD_VISIBLE
    }

    public enum MessageBoxType : uint
    {
        Error = SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR,
        Warning = SDL_MessageBoxFlags.SDL_MESSAGEBOX_WARNING,
        Information = SDL_MessageBoxFlags.SDL_MESSAGEBOX_INFORMATION
    }
}
