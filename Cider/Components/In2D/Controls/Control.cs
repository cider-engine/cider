using System;
using System.Collections.Generic;
using System.Numerics;

namespace Cider.Components.In2D.Controls
{
    public abstract class Control : Component2D, ISizeMeasurable
    {
        protected static bool SetIfChanged<T>(ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;

            return true;
        }

        public abstract bool TryMeasureSize(out Vector2 unscaledSize);
    }
}
