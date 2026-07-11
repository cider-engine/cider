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

        [SettingGroup(Display)]
        public Color BackgroundColor { get; init; } = Color.Black;

        [SettingGroup(Display)]
        public Size LogicalSize { get; init; }

        [SettingGroup(Display)]
        public LogicalPresentationMode LogicalPresentationMode { get; init; }
    }
}
