using Cider.Components;
using Cider.Data;
using Cider.Extensions;
using Cider.Input;
using Cider.Internals;
using Cider.Project;
using Cider.Render;
using Cider.Threading;
using SDL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using static SDL.SDL3;

namespace Cider
{
    public class Game
    {
        private bool _initialized;

        private double _accumulator;

        private const double _fixedTimeStep = 1.0 / 60.0;

        private long _lastTick;

        public int CurrentFps { get; private set; }

        private int _frameCount;

        private double _fpsAccumulator;

        private readonly TaskCompletionSource _gameProcess = new();

        private readonly List<Action> _endOfFrameContinuations = new(64);

        private bool _isEndOfFrame = false;

        internal static float LogicalUnitPerPhysicsUnit { get; set; } = 10;
#nullable disable
        /// <summary>
        /// <c>Game</c>的全局单例，在实例化前访问会得到<c>null</c>
        /// </summary>
        public static Game Instance { get; private set; }
#nullable restore
        /// <summary>
        /// 指示<c>Game</c>实例是否初始化完成，在<c>Game</c>对象实例化前访问会返回<c>false</c>
        /// </summary>
        public static bool IsInitialized => Instance?._initialized ?? false;

        /// <summary>
        /// MainWindow.Scene的别名
        /// </summary>
        public Scene CurrentScene
        {
            get => MainWindow.Scene;
            set => MainWindow.Scene = value;
        }

        /// <summary>
        /// <c>Game</c>所使用的项目设置
        /// </summary>
        public ProjectSettings ProjectSettings { get; }

        /// <summary>
        /// 当前是否在物理帧内
        /// </summary>
        public bool IsInPhysicsFrame { get; private set; }
#nullable disable
        /// <summary>
        /// 主窗口
        /// </summary>
        public Window MainWindow { get; private set; }

        /// <summary>
        /// 当前所使用的同步上下文
        /// </summary>
        public CiderSynchronizationContext CurrentSynchronizationContext { get; private set; }

        /// <summary>
        /// 可使用<c>ConfigureServices</c>配置的服务对象，在<c>ConfigureServices</c>调用前返回null
        /// </summary>
        public IServiceProvider Services { get; private set; }

        /// <summary>
        /// 当内置的FPS计时器变动时触发的事件
        /// </summary>
        public event EventHandler<Game, int> FpsChanged;
#nullable restore

        public Game(ProjectSettings settings)
        {
            //Instance?.Dispose();
            if (Instance is not null) throw new InvalidOperationException("The Game instance has been created.");
            Instance = this;

            ProjectSettings = settings ?? throw new NullReferenceException("ProjectSettings should not be null.");
        }

        /// <summary>
        /// <para>以同步方式运行</para>
        /// <para>浏览器端不支持以同步方式运行，必须改用<c>RunAsync</c></para>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException">当前平台为浏览器，请改用<c>RunAsync</c></exception>
        /// <exception cref="CiderGameException">游戏内异常</exception>
        [UnsupportedOSPlatform("browser")]
        public unsafe int Run()
        {
            if (OperatingSystem.IsBrowser()) throw new PlatformNotSupportedException("use RunAsync instead.");
            var result = SDL_RunApp(0, null, &Main, nint.Zero);
            if (_gameProcess.Task.Exception is { } e)
            {
                Console.Error.WriteLine(e);
                throw new CiderGameException("an exception was thrown.", e);
            }
            return result;
        }

        /// <summary>
        /// 以异步方式运行
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CiderGameException">游戏内异常</exception>
        public async Task<int> RunAsync()
        {
            int result;

            unsafe
            {
                result = SDL_RunApp(0, null, &Main, nint.Zero);
            }
            try
            {
                await _gameProcess.Task;
                return result;
            }
            catch (Exception e)
            {
                if (_gameProcess.Task.IsCanceled)
                    return result;

                throw new CiderGameException("an exception was thrown.", e);
            }
        }

        public bool TryRaiseException(Exception exception)
        {
            return _gameProcess.TrySetException(exception);
        }

        public Game ConfigureServices(Func<Game, IServiceProvider> serviceProviderFactory)
        {
            if (Services is not null) throw new InvalidOperationException($"{nameof(Services)} has been configured.");
            Services = serviceProviderFactory.Invoke(this);
            return this;
        }

        public static TaskScheduler GetTaskScheduler() => OperatingSystem.IsBrowser() ? TaskScheduler.Default : TaskScheduler.FromCurrentSynchronizationContext();

        public static void Warning(string message)
        {
            Console.Error.WriteLine(message);
        }

        void Initialize()
        {
            // 必要设置
            SynchronizationContext.SetSynchronizationContext(CurrentSynchronizationContext = new CiderSynchronizationContext());


            MainWindow = new Window(ProjectSettings.MainWindowTitle,
                ProjectSettings.MainScene,
                ProjectSettings.MainWindowSize.Width,
                ProjectSettings.MainWindowSize.Height,
                ProjectSettings.MainWindowFlags)
            {
                BackgroundColor = ProjectSettings.MainWindowBackgroundColor,
                ClearColor = ProjectSettings.MainWindowClearColor
            };

            MainWindow.Renderer.SetLogicalPresentation(ProjectSettings.MainWindowLogicalSize, ProjectSettings.MainWindowLogicalPresentationMode);

            ProjectSettings.MainWindowIcon?.LoadSurfaceAsync().ContinueWith(x =>
            {
                x.EnsureSuccess();
                MainWindow.Icon = x.Result;
            }, GetTaskScheduler());

            _initialized = true;
            CurrentScene.OnLoadedDispatcher(CurrentScene);
        }

        void Update(TimeContext context)
        {
            _frameCount++;
            _fpsAccumulator += context.DeltaTime.TotalSeconds;

            if (_fpsAccumulator >= 1.0)
            {
                FpsChanged?.Invoke(this, CurrentFps = (int)Math.Round(_frameCount / _fpsAccumulator));
                _frameCount = 0;
                _fpsAccumulator = 0;
            }

            foreach (var window in Window.AllWindows)
            {
                if (window.IsClosed) continue;

                var currentScene = window.Scene;

                currentScene.OnEarlyUpdate();

                _accumulator += context.DeltaTime.TotalSeconds;

                while (_accumulator >= _fixedTimeStep)
                {
                    // 我在想要不要加try finally
                    IsInPhysicsFrame = true;
                    currentScene.OnPhysicsStep((float)_fixedTimeStep);
                    _accumulator -= _fixedTimeStep;
                    currentScene.OnFixedUpdateDispatcher(new(TimeSpan.FromSeconds(_fixedTimeStep)));
                    IsInPhysicsFrame = false;

                    InputManager.FixedUpdate();
                }

                currentScene.OnUpdateDispatcher(context);

                currentScene.OnLateUpdate();

                Draw(window, context);
            }

            // 我在想要不要加try finally
            _isEndOfFrame = true;
            foreach (var continuation in _endOfFrameContinuations) continuation.Invoke();
            _endOfFrameContinuations.Clear();
            _isEndOfFrame = false;

            InputManager.Update();
        }

        unsafe void Draw(Window window, TimeContext context)
        {
            using (var colorScope = new RenderDrawColorScope(window.Renderer, window.ClearColor))
            {
                SDLHelpers.ThrowIfFalse(SDL_RenderClear(window.Renderer.Pointer));
            }

            using (var colorScope = new RenderDrawColorScope(window.Renderer, window.BackgroundColor))
            {
                var size = window.Renderer.CurrentOutputSize;
                SDL_FRect rect = new()
                {
                    x = 0,
                    y = 0,
                    w = size.Width,
                    h = size.Height
                };
                SDLHelpers.ThrowIfFalse(SDL_RenderFillRect(window.Renderer.Pointer, &rect));
            }

            window.Scene.OnRenderDispatcher(new()
            {
                Renderer = window.Renderer,
                TimeContext = context
            });

            SDLHelpers.ThrowIfFalse(SDL_RenderPresent(window.Renderer.Pointer));
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        unsafe static int Main(int argc, byte** argv)
        {
            return SDL_EnterAppMainCallbacks(argc, argv, &Init, &Iterate, &Event, &Quit);
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        unsafe static SDL_AppResult Init(nint* state, int argc, byte** argv)
        {
            try
            {
                SDLHelpers.ThrowIfFalse(SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO | SDL_InitFlags.SDL_INIT_AUDIO));
                SDLHelpers.ThrowIfFalse(SDL3_mixer.MIX_Init());
                SDLHelpers.ThrowIfFalse(SDL3_ttf.TTF_Init());
                Instance.Initialize();
                Instance._lastTick = Stopwatch.GetTimestamp();
                return SDL_AppResult.SDL_APP_CONTINUE;
            }
            catch (Exception e)
            {
                Instance._gameProcess.TrySetException(e);
                return SDL_AppResult.SDL_APP_FAILURE;
            }
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static SDL_AppResult Iterate(nint state)
        {
            try
            {
                // 为什么每次迭代同步上下文都没了
                //if (OperatingSystem.IsBrowser() && SynchronizationContext.Current is null)
                //    SynchronizationContext.SetSynchronizationContext(Instance.CurrentSynchronizationContext);

                var currentTick = Stopwatch.GetTimestamp();
                var context = new TimeContext(Stopwatch.GetElapsedTime(Instance._lastTick, currentTick));
                Instance.Update(context);
                Instance._lastTick = currentTick;
                return SDL_AppResult.SDL_APP_CONTINUE;
            }
            catch (Exception e)
            {
                Instance._gameProcess.TrySetException(e);
                return SDL_AppResult.SDL_APP_FAILURE;
            }
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        unsafe static SDL_AppResult Event(nint state, SDL_Event* e)
        {
            try
            {
                // 为什么每次事件同步上下文都没了
                //if (OperatingSystem.IsBrowser() && SynchronizationContext.Current is null)
                //    SynchronizationContext.SetSynchronizationContext(Instance.CurrentSynchronizationContext);

                switch (e->Type)
                {
                    case SDL_EventType.SDL_EVENT_QUIT:
                        {
                            if (Instance.MainWindow.TryClose())
                                return SDL_AppResult.SDL_APP_SUCCESS;

                            else
                                return SDL_AppResult.SDL_APP_CONTINUE;
                        }

                    #region Mouse Event
                    case SDL_EventType.SDL_EVENT_MOUSE_MOTION:
                        {
                            var @event = e->motion;
                            var rawPosition = new Vector2(@event.x, @event.y);
                            var rawMovement = new Vector2(@event.xrel, @event.yrel);
                            var window = @event.windowID.RelativeWindow;
                            if (window is not null) SDL_ConvertEventToRenderCoordinates(window.Renderer.Pointer, (SDL_Event*)&@event);
                            var args = new MouseMovedEventArgs(
                                Position: new(@event.x, @event.y),
                                RawPosition: rawPosition,
                                Movement: new(@event.xrel, @event.yrel),
                                RawMovement: rawMovement,
                                Timestamp: new(@event.timestamp),
                                MouseId: new((uint)@event.which),
                                ButtonState: (MouseButtonFlags)@event.state);
                            InputManager.RaiseMouseMoved(window, args);
                            break;
                        }

                    case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                        {
                            var @event = e->button;
                            var rawPosition = new Vector2(@event.x, @event.y);
                            var window = @event.windowID.RelativeWindow;
                            if (window is not null) SDL_ConvertEventToRenderCoordinates(window.Renderer.Pointer, (SDL_Event*)&@event);
                            var args = new MouseButtonEventArgs(
                                Position: new(@event.x, @event.y),
                                RawPosition: rawPosition,
                                Timestamp: new(@event.timestamp),
                                MouseId: new((uint)@event.which),
                                Button: (MouseButton)@event.button,
                                IsDown: @event.down,
                                ClickTimes: @event.clicks
                            );
                            InputManager.RaiseMouseDown(window, args);
                            break;
                        }

                    case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                        {
                            var @event = e->button;
                            var rawPosition = new Vector2(@event.x, @event.y);
                            var window = @event.windowID.RelativeWindow;
                            if (window is not null) SDL_ConvertEventToRenderCoordinates(window.Renderer.Pointer, (SDL_Event*)&@event);
                            var args = new MouseButtonEventArgs(
                                Position: new(@event.x, @event.y),
                                RawPosition: rawPosition,
                                Timestamp: new(@event.timestamp),
                                MouseId: new((uint)@event.which),
                                Button: (MouseButton)@event.button,
                                IsDown: @event.down,
                                ClickTimes: @event.clicks
                            );
                            InputManager.RaiseMouseUp(window, args);
                            break;
                        }
                    #endregion

                    #region Window Event
                    case SDL_EventType.SDL_EVENT_WINDOW_SHOWN:
                        {
                            e->window.windowID.RelativeWindow!.OnShown();
                            break;
                        }

                    case SDL_EventType.SDL_EVENT_WINDOW_HIDDEN:
                        {
                            e->window.windowID.RelativeWindow!.OnHidden();
                            break;
                        }

                    case SDL_EventType.SDL_EVENT_WINDOW_MOVED:
                        {
                            e->window.windowID.RelativeWindow!.OnMoved(new(e->window.data1, e->window.data2));
                            break;
                        }

                    case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                        {
                            e->window.windowID.RelativeWindow!.OnResized(new(e->window.data1, e->window.data2));
                            break;
                        }

                    case SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                        {
                            e->window.windowID.RelativeWindow!.TryClose();
                            break;
                        }
                    #endregion

                    #region Keyboard Event
                    case SDL_EventType.SDL_EVENT_KEY_DOWN:
                        {
                            var @event = e->key;
                            var args = new KeyboardEventArgs(Timestamp: new(@event.timestamp),
                                Code: (KeyCode)@event.scancode,
                                Symbol: (KeySymbol)@event.key,
                                Modifier: (KeyModifier)@event.mod,
                                Raw: @event.raw,
                                IsDown: @event.down,
                                IsRepeat: @event.repeat);
                            InputManager.RaiseKeyDown(@event.windowID.RelativeWindow, args);
                            break;
                        }

                    case SDL_EventType.SDL_EVENT_KEY_UP:
                        {
                            var @event = e->key;
                            var args = new KeyboardEventArgs(Timestamp: new(@event.timestamp),
                                Code: (KeyCode)@event.scancode,
                                Symbol: (KeySymbol)@event.key,
                                Modifier: (KeyModifier)@event.mod,
                                Raw: @event.raw,
                                IsDown: @event.down,
                                IsRepeat: @event.repeat);
                            InputManager.RaiseKeyUp(@event.windowID.RelativeWindow, args);
                            break;
                        }
                    #endregion

                    default:
                        {
                            if (e->type == CiderSynchronizationContext.EventType)
                                while (Instance.CurrentSynchronizationContext.Tasks.TryTake(out var task))
                                {
                                    try
                                    {
                                        task.d.Invoke(task.state);
                                    }
                                    catch (Exception exc)
                                    {
                                        Instance._gameProcess.TrySetException(exc);
                                        return SDL_AppResult.SDL_APP_FAILURE;
                                    }
                                }
                            break;
                        }
                }
                return SDL_AppResult.SDL_APP_CONTINUE;
            }
            catch (Exception exc)
            {
                Instance._gameProcess.TrySetException(exc);
                return SDL_AppResult.SDL_APP_FAILURE;
            }
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Quit(nint state, SDL_AppResult result)
        {
            Debug.Assert(result == SDL_AppResult.SDL_APP_SUCCESS);
            SDL3_ttf.TTF_Quit();
            SDL3_mixer.MIX_Quit();
            Instance._gameProcess.TrySetResult();
        }


        public readonly struct EndOfFrameAwaitable
        {
            public EndOfFrameAwaiter GetAwaiter() => new();
        }

        public readonly struct EndOfFrameAwaiter : ICriticalNotifyCompletion
        {
            // 即时返回，已经在帧末时直接返回true，不会与遍历冲突
            public bool IsCompleted => Instance._isEndOfFrame;

            public void GetResult()
            {
                if (!IsCompleted) throw new InvalidOperationException("Calling GetResult when IsCompleted is false is invalid");
            }

            public void OnCompleted(Action continuation)
            {
                if (continuation is not null) Instance._endOfFrameContinuations.Add(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                if (continuation is not null) Instance._endOfFrameContinuations.Add(continuation);
            }
        }
    }

    /// <summary>
    /// <para>游戏时间戳，单位为纳秒</para>
    /// <para>支持作差得到<c>TimeSpan</c>，但<c>TimeSpan的最小单位为1刻即100纳秒，会产生精度丢失</c></para>
    /// </summary>
    /// <param name="Nanoseconds"></param>
    public readonly record struct GameTimestamp(ulong Nanoseconds)
    {
        public static TimeSpan operator -(GameTimestamp left, GameTimestamp right) =>
            TimeSpan.FromTicks(unchecked((long)(left.Nanoseconds - right.Nanoseconds)) / TimeSpan.NanosecondsPerTick);

        public static bool operator >(GameTimestamp left, GameTimestamp right) => left.Nanoseconds > right.Nanoseconds;

        public static bool operator >=(GameTimestamp left, GameTimestamp right) => left.Nanoseconds >= right.Nanoseconds;

        public static bool operator <(GameTimestamp left, GameTimestamp right) => left.Nanoseconds < right.Nanoseconds;

        public static bool operator <=(GameTimestamp left, GameTimestamp right) => left.Nanoseconds <= right.Nanoseconds;
        
        // ==和!=用record自动生成的
    }
}
