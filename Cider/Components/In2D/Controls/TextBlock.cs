using Cider.Assets;
using Cider.Attributes;
using Cider.Data;
using Cider.Data.In2D;
using Cider.Input;
using Cider.Render.In2D;
using Cider.Extensions;
using FontStashSharp;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Cider.Components.In2D.Controls
{
    [UnstableApi]
    public class TextBlock : Control
    {
#nullable enable
        public FontAsset? Font { get; set; }
#nullable disable

        public float FontSize { get; set; } = 64;

        [NotNull]
        public string Text { get; set => field = value ?? throw new NullReferenceException(); } = string.Empty;

        /// <summary>
        /// <see cref="Text"/>属性的别名
        /// </summary>
        [NotNull]
        public string Content { get => Text; set => Text = value; }

        public Color Background { get; set; } = Color.Transparent;

        private (FontAsset font, float fontSize, string text, Vector2 scale) _cachedMeasureParams = default;

        private float measuredWidth;

        private float measuredHeight;

        private void UpdateMeasuredSize(Vector2 scale)
        {
            if (Font is null || FontSize == 0 || scale.X == 0 || scale.Y == 0)
            {
                measuredWidth = 0;
                measuredHeight = 0;
                return;
            }
            var (width, height) = Font.Get().GetFont(FontSize).MeasureString(Text, scale);
            measuredWidth = width;
            measuredHeight = height;
        }

        protected internal override bool HitTest(HitTestResult result)
        {
            if ((Font, FontSize, Text, result.CurrentTransform2D.Scale) != _cachedMeasureParams)
            {
                _cachedMeasureParams = (Font, FontSize, Text, result.CurrentTransform2D.Scale);
                UpdateMeasuredSize(result.CurrentTransform2D.Scale);
            }
            if (Font is null) return false;
            return RectangleHitTest(result, measuredWidth, measuredHeight);
        }

        protected override void OnDraw2D(RenderContext2D context)
        {
            var transform = context.CurrentTransform2D;
            var scale = transform.Scale;
            if ((Font, FontSize, Text, scale) != _cachedMeasureParams)
            {
                _cachedMeasureParams = (Font, FontSize, Text, scale);
                UpdateMeasuredSize(scale);
            }
            if (Font is null) return;
            context.SpriteBatch.FillRectangle(transform.Position, measuredWidth, measuredHeight, transform.RotationInRadians, Background);
            context.SpriteBatch.DrawString(Font.Get().GetFont(FontSize),
                Text,
                transform.Position, Color.White,
                transform.RotationInRadians,
                Vector2.Zero,
                scale);
        }
    }
}
