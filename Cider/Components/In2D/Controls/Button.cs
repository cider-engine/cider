using Cider.Attributes;
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

            ContentChanged += x => _content.Text = x;

            FontSizeChanged += x => _content.FontSize = x;
        }

        [NotNull]
        public string Content
        {
            get;
            set
            {
                if (SetIfChanged(ref field, value)) ContentChanged?.Invoke(value);
            }
        } = "";

        public event Action<string> ContentChanged = null!;

        public float FontSize
        {
            get;
            set
            {
                if (SetIfChanged(ref field, value)) FontSizeChanged?.Invoke(value);
            }
        } = TextBlock.DefaultFontSize;

        public event Action<float> FontSizeChanged = null!;
    }
}
