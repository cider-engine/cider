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
using System.Diagnostics;
using System.Drawing;
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

        public int CurrentFPS { get; private set; }

        private int _frameCount;

        private double _fpsAccumulator;

        private readonly TaskCompletionSource<Exception> _exception = new();

        public static Game Instance { get; private set; }

        public static bool IsInitialized => Instance?._initialized ?? false;

        /// <summary>
        /// MainWindow.Scene的别名
        /// </summary>
        public Scene CurrentScene
        {
            get => MainWindow.Scene;
            set => MainWindow.Scene = value;
        }

        public ProjectSettings ProjectSettings { get; private set; }

        public Window MainWindow { get; private set; }

        public CiderSynchronizationContext CurrentSynchronizationContext { get; private set; }

        public IServiceProvider Services { get; private set; }

        public Game(ProjectSettings settings)
        {
            //Instance?.Dispose();
            Instance = this;

            ProjectSettings = settings;
        }

        [UnsupportedOSPlatform("browser")]
        public unsafe int Run()
        {
            if (OperatingSystem.IsBrowser()) throw new PlatformNotSupportedException("use RunAsync instead.");
            var result = SDL_RunApp(0, null, &Main, nint.Zero);
            if (_exception.Task.IsCompletedSuccessfully)
            {
                Console.Error.WriteLine(_exception.Task.Result);
                throw new Exception("an exception was thrown.", _exception.Task.Result);
            }
            return result;
        }

        public async Task<int> RunAsync()
        {
            if (!OperatingSystem.IsBrowser())
            {
                return Run();
            }

            int result;

            unsafe
            {
                result = SDL_RunApp(0, null, &Main, nint.Zero);
            }
            try
            {
                var e = await _exception.Task;
                Console.Error.WriteLine(e);
                throw new Exception("An exception was thrown.", e);
            }
            catch (OperationCanceledException)
            {
                return result;
            }
        }

        public bool TryRaiseException(Exception exception)
        {
            return _exception.TrySetException(exception);
        }

        public Game ConfigureServices(Func<Game, IServiceProvider> serviceProviderFactory)
        {
            if (Services is not null) throw new InvalidOperationException($"{nameof(Services)} has been configured.");
            Services = serviceProviderFactory.Invoke(this);
            return this;
        }

        void Initialize()
        {
            if (ProjectSettings is null)
                throw new InvalidOperationException("You must set project settings before initializing the game.");


            // 必要设置
            SynchronizationContext.SetSynchronizationContext(CurrentSynchronizationContext = new CiderSynchronizationContext());


            MainWindow = new Window(ProjectSettings.MainWindowTitle, ProjectSettings.MainWindowSize.Width, ProjectSettings.MainWindowSize.Height, ProjectSettings.MainWindowFlags)
            {
                Scene = ProjectSettings.MainScene
            };

            MainWindow.Renderer.SetLogicalPresentation(ProjectSettings.LogicalSize, ProjectSettings.LogicalPresentationMode);

            _initialized = true;
            MainWindow.Show();
            CurrentScene.OnLoadedDispatcher(CurrentScene);
        }

        void Update(TimeContext context)
        {
            _frameCount++;
            _fpsAccumulator += context.DeltaTime.TotalSeconds;

            if (_fpsAccumulator >= 1.0)
            {
                CurrentFPS = (int)Math.Round(_frameCount / _fpsAccumulator);
                _frameCount = 0;
                _fpsAccumulator = 0;
            }

            Window.AllWindows.FlushRemove();
            foreach (var window in Window.AllWindows.Values)
            {
                var currentScene = window.Scene;

                foreach (var item in currentScene.BodiesToRemove2D)
                {
                    currentScene.World2D.Remove(item);
                }

                currentScene.BodiesToRemove2D.Clear();

                _accumulator += context.DeltaTime.TotalSeconds;

                while (_accumulator >= _fixedTimeStep)
                {
                    currentScene.World2D.Step((float)_fixedTimeStep);
                    _accumulator -= _fixedTimeStep;
                    currentScene.OnFixedUpdateDispatcher(new(TimeSpan.FromSeconds(_fixedTimeStep)));
                }

                currentScene.OnUpdateDispatcher(context);

                foreach (var item in currentScene.BodiesToAdd2D)
                {
                    currentScene.World2D.Add(item);
                }

                currentScene.BodiesToAdd2D.Clear();

                Draw(window, context);
            }
            Window.AllWindows.FlushAdd();
        }

        unsafe void Draw(Window window, TimeContext context)
        {
            using (var colorScope = new RenderDrawColorScope(window.Renderer, ProjectSettings.BackgroundColor))
            {
                SDLHelpers.ThrowIfFalse(SDL_RenderClear(window.Renderer.Pointer));
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
                Instance._exception.SetResult(e);
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
                var context = new TimeContext(Stopwatch.GetElapsedTime(Instance!._lastTick, currentTick));
                Instance!.Update(context);
                Instance!._lastTick = currentTick;
                return SDL_AppResult.SDL_APP_CONTINUE;
            }
            catch (Exception e)
            {
                Instance._exception.SetResult(e);
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
                            InputManager.RaiseMouseMoved(e->motion.windowID.RelativeWindow, e->motion);
                            break;
                        }

                    case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                        {
                            InputManager.RaiseMouseDown(e->button.windowID.RelativeWindow, e->button);
                            break;
                        }

                    case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                        {
                            InputManager.RaiseMouseUp(e->button.windowID.RelativeWindow, e->button);
                            break;
                        }
                    #endregion

                    #region Window Event
                    case SDL_EventType.SDL_EVENT_WINDOW_SHOWN:
                        {
                            if (e->window.windowID.TryGetWindow(out var window)) window.OnShown();
                            break;
                        }

                    case SDL_EventType.SDL_EVENT_WINDOW_HIDDEN:
                        {
                            if (e->window.windowID.TryGetWindow(out var window)) window.OnHidden();
                            break;
                        }

                    case SDL_EventType.SDL_EVENT_WINDOW_MOVED:
                        {
                            if (e->window.windowID.TryGetWindow(out var window)) window.OnMoved(new(e->window.data1, e->window.data2));
                            break;
                        }

                    case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                        {
                            if (e->window.windowID.TryGetWindow(out var window)) window.OnResized(new(e->window.data1, e->window.data2));
                            break;
                        }

                    case SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                        {
                            if (e->window.windowID.TryGetWindow(out var window)) window.TryClose();
                            break;
                        }
                    #endregion

                    #region Keyboard Event
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
                                        Instance._exception.SetResult(exc);
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
                Instance._exception.SetResult(exc);
                return SDL_AppResult.SDL_APP_FAILURE;
            }
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Quit(nint state, SDL_AppResult result)
        {
            Debug.Assert(result == SDL_AppResult.SDL_APP_SUCCESS);
            SDL3_ttf.TTF_Quit();
            SDL3_mixer.MIX_Quit();
            Instance._exception.TrySetCanceled();
        }
    }
}
