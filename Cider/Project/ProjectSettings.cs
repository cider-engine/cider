using Cider.Components;
using System;

namespace Cider.Project
{
    public class ProjectSettings
    {
        public ApplicationSettings Application { get; init; } = new();
    }

    public class ApplicationSettings
    {
        public RunSettings Run { get; init; } = new();
    }

    public class RunSettings
    {
        public Scene MainScene { get; init; }
    }
}
