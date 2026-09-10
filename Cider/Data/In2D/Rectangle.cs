using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Data.In2D
{
    public record struct Rectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public readonly int Left => X;
        public readonly int Top => Y;
        public readonly int Right => X + Width;
        public readonly int Bottom => Y + Height;
        public readonly bool IsEmpty => (Width <= 0) || (Height <= 0);

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public static Rectangle Empty => new();

        public static Rectangle FromLTRB(int left, int top, int right, int bottom) =>
            new(left, top, unchecked(right - left), unchecked(bottom - top));
    }
}
