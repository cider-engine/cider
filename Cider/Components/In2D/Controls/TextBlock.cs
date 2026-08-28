using Cider.Assets;
using Cider.Attributes;
using Cider.Data.In2D;
using Cider.Extensions;
using Cider.Input;
using Cider.Internals;
using Cider.Render;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace Cider.Components.In2D.Controls
{
    public enum TextRenderMode
    {
        Cached,
        Direct
    }

    [Content(nameof(Text))]
    public class TextBlock : Control
    {
        private unsafe delegate* managed<TextBlock, RenderContext, void> _renderFunction = &OnCachedRender;
        private Texture? _cachedTexture = null;
        private Task<FontVariant>? _fontVariant = null;
        private Text? _text = null;

        public TextRenderMode RenderMode
        {
            get;
            set
            {
                field = value;

                DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);

                unsafe
                {
                    _renderFunction = value switch
                    {
                        TextRenderMode.Cached => &OnCachedRender,
                        TextRenderMode.Direct => &OnDirectRender,
                        _ => throw new InvalidOperationException("The mode is invalid")
                    };
                }
            }
        } = TextRenderMode.Direct;

        public FontAsset? Font
        {
            get => field ?? Game.Instance?.ProjectSettings.DefaultFallbackFont;
            set
            {
                if (field == value) return;

                DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);
                DisposableHelpers.DisposeAndSetNull(ref _fontVariant);
                DisposableHelpers.DisposeAndSetNull(ref _text);

                field = value;

                if (Game.IsInitialized)
                    _fontVariant = Font?.LoadAsync()
                        .ContinueWith(x => SetFontProperties(x.Result.CreateVariant()),
                            Game.GetTaskScheduler())
                        .EnsureToBeSuccessful();
            }
        }

        public const float DefaultFontSize = 64;

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
        } = DefaultFontSize;

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

        private protected override void OnWindowChangedInternal(Window? oldWindow, Window? newWindow)
        {
            DisposableHelpers.DisposeAndSetNull(ref _cachedTexture);
            //DisposableHelpers.DisposeAndSetNull(ref _fontVariant);
            DisposableHelpers.DisposeAndSetNull(ref _text);
            base.OnWindowChangedInternal(oldWindow, newWindow);
        }

        public bool TryMeasureSize(out float unscaledWidth, out float unscaledHeight)
        {
            if (_text is Text text)
            {
                var size = text.Size;
                unscaledWidth = size.Width;
                unscaledHeight = size.Height;
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
            if (_text is { Size: { IsEmpty: false, Width: var width, Height: var height } })
                return result.RectangleHitTest(width, height);

            return false;
        }

        protected override unsafe void OnRender(RenderContext context)
        {
            Debug.Assert(_renderFunction != null);
            _renderFunction(this, context);
        }

        private static void OnCachedRender(TextBlock @this, RenderContext context)
        {
            if (@this.Font is null) return;

            if (@this._cachedTexture is null)
            {
                if (@this._fontVariant is null)
                {
                    @this._fontVariant = @this.Font.LoadAsync()
                        .ContinueWith(x => @this.SetFontProperties(x.Result.CreateVariant()),
                            Game.GetTaskScheduler())
                        .EnsureToBeSuccessful();

                    return;
                }

                else if (@this._fontVariant is { IsCompletedSuccessfully: true } task)
                {
                    var text = @this._text ??= new Text(context.Renderer.TextEngine.Value, task.Result, @this.Text);

                    text.Color = @this.Foreground;

                    var size = text.Size;

                    if (size.IsEmpty) return;

                    @this._cachedTexture = new(context.Renderer, size.Width, size.Height, TextureAccess.Target);

                    using (context.PushTarget(@this._cachedTexture))
                    {
                        context.FillColor(@this.Background);
                        text.Render(0, 0);
                    }
                }

                else return;
            }

            var transform = @this.GlobalTransform;
            //context.FillRectangle(transform.Position, measuredWidth, measuredHeight, transform.RotationInDegrees, Background, transform.Scale);
            context.RenderTexture(@this._cachedTexture, transform.Position, null, transform.RotationInDegrees, transform.Scale, Vector2.Zero, FlipMode.None);
        }

        private static void OnDirectRender(TextBlock @this, RenderContext context)
        {
            if (@this.Font is null) return;

            if (@this._fontVariant is null)
            {
                @this._fontVariant = @this.Font.LoadAsync()
                    .ContinueWith(x => @this.SetFontProperties(x.Result.CreateVariant()),
                        Game.GetTaskScheduler())
                    .EnsureToBeSuccessful();

                return;
            }

            else if (@this._fontVariant is { IsCompletedSuccessfully: true } task)
            {
                var text = @this._text ??= new Text(context.Renderer.TextEngine.Value, task.Result, @this.Text);

                text.Color = @this.Foreground;

                var transform = @this.GlobalTransform;

                if (transform.RotationInRadians != 0) Game.Warning("Direct render does not support rotation");

                if (transform.Scale != Vector2.One) Game.Warning("Direct render doest not support scale");

                var size = text.Size;

                context.FillRectangle(transform.Position, size.Width, size.Height, 0, @this.Background, Vector2.One);

                if (context.Renderer.Camera2D is { IsEnabled: true } camera)
                    text.Render(transform.Position.X - camera.OffsetPosition.X, transform.Position.Y - camera.OffsetPosition.Y);

                else
                    text.Render(transform.Position.X, transform.Position.Y);
            }
        }
    }
}
