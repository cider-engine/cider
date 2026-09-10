using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Data.In2D
{
    public record struct Size
    {
        public int Width;
        public int Height;

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public readonly bool IsEmpty => Width == 0 && Height == 0;
    }
}
