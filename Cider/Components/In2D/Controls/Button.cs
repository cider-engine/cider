using Cider.Attributes;
using Cider.Data.In2D;
using Cider.Input;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Cider.Components.In2D.Controls
{
    [Content(nameof(Content))]
    public class Button : ButtonBase
    {
        private readonly TextBlock _content;

        public Button()
        {
            Children.AddRange([(_content = new TextBlock())]);

            ContentChanged = x => _content.Text = x;

            FontSizeChanged = x => _content.FontSize = x;

            ForegroundChanged = x => _content.Foreground = x;
        }

        [NotNull]
        public string Content
        {
            get;
            set
            {
                if (SetIfChanged(ref field, value)) ContentChanged.Invoke(value);
            }
        } = "";

        public event Action<string> ContentChanged;

        public float FontSize
        {
            get;
            set
            {
                if (SetIfChanged(ref field, value)) FontSizeChanged.Invoke(value);
            }
        } = TextBlock.DefaultFontSize;

        public event Action<float> FontSizeChanged;

        public Color Foreground
        {
            get;
            set
            {
                if (SetIfChanged(ref field, value)) ForegroundChanged.Invoke(value);
            }
        }

        public event Action<Color> ForegroundChanged;

        protected override bool HitTest(HitTestResult result)
        {
            if (_content.TryMeasureSize(out var width, out var height))
                return result.RectangleHitTest(width, height);

            return false;
        }
    }
}
