using System;
using System.Collections.Generic;

namespace Cider.Components.In2D.Controls
{
    public class Control : Component2D
    {
        protected static bool SetIfChanged<T>(ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;

            return true;
        }
    }
}
