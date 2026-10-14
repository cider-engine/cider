using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Cider.Components
{
    public interface ISizeMeasurable
    {
        bool TryMeasureSize(out Vector2 unscaledSize);
    }
}
