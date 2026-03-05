using Cider.Assets;
using Cider.Attributes;
using Cider.Data.In2D;
using Cider.Extensions;
using Cider.Input;
using Cider.Internals;
using Cider.Render;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace Cider.Components.In2D.Controls
{
    [UnstableApi]
    public class TextBlock : Control
    {
#nullable enable
        private Texture? _cachedTexture = null;
        private Task<Font>? _underlyingFont = null;
        public FontAsset? Font
        {
            get;
            set
            {
                DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);
                DisposableHelpers.DisposeAndSetNull(ref _underlyingFont);
                field = value;
                if (value is not null && Game.IsInitialized)
                {
                    _underlyingFont = value.Load(FontSize);
                    _underlyingFont.ContinueWith(x =>
                    {
                        x.EnsureSuccess();
                        if (CurrentWindow is not null)
                        {
                            var font = x.Result;
                            using var surface = font.RenderShaded(Text, Foreground, Background);
                            _cachedTexture = new(CurrentWindow.Renderer, surface);
                        }
                    });
                }
            }
        }
#nullable disable

        public float FontSize
        {
            get;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, 0, nameof(FontSize));
                field = value;
            }
        } = 64;

        [NotNull]
        public string Text { get; set => field = value ?? throw new NullReferenceException(); } = string.Empty;

        /// <summary>
        /// <see cref="Text"/>属性的别名
        /// </summary>
        [NotNull]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string Content { get => Text; set => Text = value; }

        public Color Foreground { get; set; } = Color.Black;

        public Color Background { get; set; } = Color.Transparent;

        protected override void OnWindowChanged(Window oldWindow, Window newWindow)
        {
            DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);
            if (newWindow is not null)
            {
                _underlyingFont ??= Font?.Load(FontSize);
                _underlyingFont?.ContinueWith(x =>
                {
                    x.EnsureSuccess();
                    var font = x.Result;
                    using var surface = font.RenderShaded(Text, Foreground, Background);
                    _cachedTexture = new(newWindow.Renderer, surface);
                });
            }
        }

        public bool TryMeasureSize(out float unscaledWidth, out float unscaledHeight)
        {
            if (Font is null || _underlyingFont?.IsCompletedSuccessfully != true)
            {
                unscaledWidth = 0;
                unscaledHeight = 0;
                return false;
            }
            var (width, height) = _underlyingFont.Result.MeasureString(Text);
            unscaledWidth = width;
            unscaledHeight = height;
            return true;
        }

        protected override bool HitTest(HitTestResult result)
        {
            if (Font is null || _cachedTexture is null) return false;
            return RectangleHitTest(result, _cachedTexture.Width, _cachedTexture.Height); // 可点击必定已渲染，复用Texture的Width和Height
        }

        protected override void OnRender(RenderContext context)
        {
            if (_cachedTexture is null) return;

            var transform = GlobalTransform;
            if (Font is null) return;
            //context.FillRectangle(transform.Position, measuredWidth, measuredHeight, transform.RotationInDegrees, Background, transform.Scale);
            context.RenderTexture(_cachedTexture, transform.Position, null, transform.RotationInDegrees, transform.Scale, Vector2.Zero, FlipMode.None);
        }
    }
}
