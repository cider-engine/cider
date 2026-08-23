using Cider.Assets;
using Cider.Attributes;
using Cider.Components;
using Cider.Render;
using System;
using System.Drawing;
using System.Numerics;

namespace Cider.Project
{
    public class ProjectSettings
    {
        const string Application = nameof(Application);

        [SettingGroup(Application)]
        public required Scene MainScene { get; init; }

        const string Display = nameof(Display);

        [SettingGroup(Display)]
        public string MainWindowTitle { get; init; } = string.Empty;

        [SettingGroup(Display)]
        public WindowFlags MainWindowFlags { get; init; }

        [SettingGroup(Display)]
        public Size MainWindowSize { get; init; }

        [SettingGroup(Display)]
        public TextureAsset? MainWindowIcon { get; init; }

        [SettingGroup(Display)]
        public Color MainWindowBackgroundColor { get; init; } = Color.Black;

        [SettingGroup(Display)]
        public Color MainWindowClearColor { get; init; } = Color.Black;

        [SettingGroup(Display)]
        public Size MainWindowLogicalSize { get; init; }

        [SettingGroup(Display)]
        public LogicalPresentationMode MainWindowLogicalPresentationMode { get; init; }

        const string UI = nameof(UI);

        [SettingGroup(UI)]
        public FontAsset? DefaultFallbackFont { get; init; }

        const string Render = nameof(Render);

        const string Physics = nameof(Physics);

        [SettingGroup(Physics)]
        public Vector2 DefaultGravity { get; set; } = new(0, 0.98f * Game.LogicalUnitPerPhysicsUnit);
    }
}
