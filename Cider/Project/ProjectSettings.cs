using Cider.Assets;
using Cider.Attributes;
using Cider.Components;
using Cider.Render;
using System;
using System.Drawing;

namespace Cider.Project
{
    public class ProjectSettings
    {
        const string Application = nameof(Application);

        [SettingGroup(Application)]
        public Scene MainScene { get; init; }

        const string Display = nameof(Display);

        [SettingGroup(Display)]
        public string MainWindowTitle { get; init; } = string.Empty;

        [SettingGroup(Display)]
        public WindowFlags MainWindowFlags { get; init; }

        [SettingGroup(Display)]
        public Size MainWindowSize { get; init; }
#nullable enable
        [SettingGroup(Display)]
        public TextureAsset? MainWindowIcon { get; init; }
#nullable disable
        [SettingGroup(Display)]
        public Color BackgroundColor { get; set; } = Color.Black;

        [SettingGroup(Display)]
        public Color ClearColor { get; set; } = Color.Black;

        [SettingGroup(Display)]
        public Size LogicalSize { get; init; }

        [SettingGroup(Display)]
        public LogicalPresentationMode LogicalPresentationMode { get; init; }
    }
}
