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
        private Task<FontVariant>? _fontVariant = null;
        private Text? _text = null;

        public FontAsset? Font
        {
            get;
            set
            {
                if (field == value) return;

                DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);
                DisposableHelpers.DisposeAndSetNull(ref _fontVariant);
                DisposableHelpers.DisposeAndSetNull(ref _text);

                field = value;

                if (Game.IsInitialized)
                    _fontVariant = Font?.Load()
                        .ContinueWith(x => SetFontProperties(x.Result.CreateVariant()),
                            TaskScheduler.FromCurrentSynchronizationContext())
                        .EnsureToBeSuccessful();
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
                DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);
                if (_fontVariant is { IsCompletedSuccessfully: true } task) task.Result.FontSize = value;
            }
        } = 64;

        public FontStyleFlags FontStyle
        {
            get;
            set
            {
                field = value;
                DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);
                if (_fontVariant is { IsCompletedSuccessfully: true } task) task.Result.FontStyle = value;
            }
        }

        public int FontOutline
        {
            get;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(FontOutline));
                field = value;
                DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);
                if (_fontVariant is { IsCompletedSuccessfully: true } task) task.Result.Outline = value;
            }
        }

        public FontLineJoin FontOutlineLineJoin
        {
            get;
            set
            {
                field = value;
                DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);
                if (_fontVariant is { IsCompletedSuccessfully: true } task) task.Result.OutlineLineJoin = value;
            }
        }

        public FontLineCap FontOutlineLineCap
        {
            get;
            set
            {
                field = value;
                DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);
                if (_fontVariant is { IsCompletedSuccessfully: true } task) task.Result.OutlineLineCap = value;
            }
        }

        public int FontCharSpacing
        {
            get;
            set
            {
                field = value;
                DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);
                if (_fontVariant is { IsCompletedSuccessfully: true } task) task.Result.CharSpacing = value;
            }
        }

        [NotNull]
        public string Text
        {
            get;
            set
            {
                var isDifferent = field != value;
                field = value ?? throw new NullReferenceException();
                DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);
                if (_text is Text text && isDifferent) text.SetContent(value);
            }
        } = string.Empty;

        /// <summary>
        /// <see cref="Text"/>属性的别名
        /// </summary>
        [NotNull]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string Content { get => Text; set => Text = value; }

        public Color Foreground
        {
            get;
            set
            {
                field = value;
                DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);
                if (_text is Text text) text.Color = value;
            }
        } = Color.Black;

        public Color Background { get; set; } = Color.Transparent;

        FontVariant SetFontProperties(FontVariant font)
        {
            font.FontSize = FontSize;
            font.FontStyle = FontStyle;
            font.Outline = FontOutline;
            font.OutlineLineJoin = FontOutlineLineJoin;
            font.OutlineLineCap = FontOutlineLineCap;
            font.CharSpacing = FontCharSpacing;

            return font;
        }

        protected override void OnWindowChanged(Window oldWindow, Window newWindow)
        {
            DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);
            //DisposableHelpers.DisposeAndSetNull(ref _fontVariant);
            DisposableHelpers.DisposeAndSetNull(ref _text);
        }

        public bool TryMeasureSize(out float unscaledWidth, out float unscaledHeight)
        {
            if (_text is Text text)
            {
                text.Measure(out int width, out int height);
                unscaledWidth = width;
                unscaledHeight = height;
                return true;
            }

            else
            {
                unscaledWidth = 0;
                unscaledHeight = 0;
                return false;
            }
        }

        protected override bool HitTest(HitTestResult result)
        {
            if (Font is null || _cachedTexture is null) return false;
            return RectangleHitTest(result, _cachedTexture.Width, _cachedTexture.Height); // 可点击必定已渲染，复用Texture的Width和Height
        }

        protected override void OnRender(RenderContext context)
        {
            if (Font is null) return;

            if (_cachedTexture is null)
            {
                if (_fontVariant is null)
                {
                    _fontVariant = Font.Load()
                        .ContinueWith(x => SetFontProperties(x.Result.CreateVariant()),
                            TaskScheduler.FromCurrentSynchronizationContext())
                        .EnsureToBeSuccessful();

                    return;
                }

                else if (_fontVariant is { IsCompletedSuccessfully: true } task)
                {
                    var text = _text ??= new Text(context.Renderer.TextEngine.Value, task.Result, Text);

                    text.Color = Foreground;

                    text.Measure(out int width, out int height);

                    if (width == 0 || height == 0) return;

                    _cachedTexture = new(context.Renderer, width, height, TextureAccess.Target);

                    //using (context.PushBlendMode(BlendMode.None))
                    using (context.PushTarget(_cachedTexture))
                    {
                        context.FillColor(Background);
                        text.Render(0, 0);
                    }
                }

                else return;
            }

            var transform = GlobalTransform;
            //context.FillRectangle(transform.Position, measuredWidth, measuredHeight, transform.RotationInDegrees, Background, transform.Scale);
            context.RenderTexture(_cachedTexture, transform.Position, null, transform.RotationInDegrees, transform.Scale, Vector2.Zero, FlipMode.None);
        }
    }
}
