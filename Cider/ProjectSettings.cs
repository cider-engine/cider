using Cider.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider
{
    public class ProjectSettings
    {
        public static ProjectSettings Current => CiderGame.Instance.ProjectSettings;

        public Scene MainScene { get; init; }
    }
}
