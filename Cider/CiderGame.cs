using Cider.Components;
using Cider.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoStereo;
using System;

namespace Cider
{
    public class CiderGame : Game
    {
        private bool _initialized;

        private bool _disposed;

        private double _accumulator;

        private const double _fixedTimeStep = 1.0 / 60.0;

        public static CiderGame Instance { get; private set; }

        protected GraphicsDeviceManager GraphicsDeviceManager { get; private set; }

        protected SpriteBatch SpriteBatch { get; private set; }

        public Scene CurrentScene
        {
            get;
            set
            {
                if (_initialized)
                {
                    value.OnParentTransformChanged(EventArgs.Empty);
                    value.OnLoaded(value); // 如果游戏没有初始化，则在Initialize里调用
                }

                field = value;
            }
        }

        public bool IsFocused { get; private set; }

        public CiderGame(ProjectSettings settings)
        {
            Instance?.Dispose();
            Instance = this;
            GraphicsDeviceManager = new GraphicsDeviceManager(this);
            // Content.RootDirectory = "Content";
            IsMouseVisible = true;

            CurrentScene = settings.MainScene;

            Window.AllowUserResizing = true;

            Window.Position = Window.Position; // 莫名其妙试出来的防止第一次改变窗口大小时改变缓冲区会使窗口反复居中频闪的方法
        }

        [Obsolete("The Content property is available but shouldn't be used")]
        public new ContentManager Content { get => base.Content; set => base.Content = value; }

        protected override void Initialize()
        {
            base.Initialize();
            MonoStereoEngine.Initialize(() => _disposed);
            CurrentScene.OnParentTransformChanged(EventArgs.Empty);
            CurrentScene.OnLoaded(CurrentScene);
            _initialized = true;
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            MonoStereoEngine.ThrowIfErrored();

            InputManager.Update(gameTime);

            foreach (var item in CurrentScene.BodiesToRemove2D)
            {
                CurrentScene.World2D.Remove(item);
            }

            CurrentScene.BodiesToRemove2D.Clear();

            _accumulator += gameTime.ElapsedGameTime.TotalSeconds;

            while (_accumulator >= _fixedTimeStep)
            {
                CurrentScene.World2D.Step((float)_fixedTimeStep);
                _accumulator -= _fixedTimeStep;
                CurrentScene.OnFixedUpdate(new Cider.Data.TimeContext(TimeSpan.FromSeconds(_fixedTimeStep)));
            }

            var timeContext = new Cider.Data.TimeContext(gameTime.ElapsedGameTime);

            CurrentScene.OnUpdate(timeContext);

            foreach (var item in CurrentScene.BodiesToAdd2D)
            {
                CurrentScene.World2D.Add(item);
            }

            CurrentScene.BodiesToAdd2D.Clear();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // 防止窗口大小改变后画面拉伸
            if (Window.ClientBounds.Width != GraphicsDeviceManager.PreferredBackBufferWidth ||
                Window.ClientBounds.Height != GraphicsDeviceManager.PreferredBackBufferHeight)
            {
                GraphicsDeviceManager.PreferredBackBufferWidth = Window.ClientBounds.Width;
                GraphicsDeviceManager.PreferredBackBufferHeight = Window.ClientBounds.Height;
                GraphicsDeviceManager.ApplyChanges();
            }

            GraphicsDevice.Clear(Color.CornflowerBlue);

            SpriteBatch.Begin();

            CurrentScene.OnDraw(new()
            {
                GameTime = gameTime,
                SpriteBatch = SpriteBatch
            });

            SpriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            IsFocused = true;
            base.OnActivated(sender, args);
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            IsFocused = false;
            base.OnDeactivated(sender, args);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;
            Instance = null;
            SpriteBatch.Dispose();
            GraphicsDeviceManager.Dispose();
            base.Dispose(disposing);
        }
    }
}
