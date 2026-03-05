using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Data.In2D
{
    [Flags]
    public enum FlipMode
    {
        None = 0,
        FlipHorizontally = 0b01,
        FlipVertically = 0b10,
        FlipHorizontallyAndVertically = 0b11
    }
}
