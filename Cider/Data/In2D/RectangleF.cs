using Cider.Render;
using System;

namespace Cider.Data.In2D
{
    public record struct RectangleF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
        public readonly float Left => X;
        public readonly float Top => Y;
        public readonly float Right => X + Width;
        public readonly float Bottom => Y + Height;
        public readonly bool IsEmpty => (Width <= 0) || (Height <= 0);

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public readonly RectangleF SubRectangle(RectangleF subregion)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(subregion.X);
            ArgumentOutOfRangeException.ThrowIfNegative(subregion.Y);
            ArgumentOutOfRangeException.ThrowIfLessThan(Width, subregion.X + subregion.Width);
            ArgumentOutOfRangeException.ThrowIfLessThan(Height, subregion.Y + subregion.Height);
            return new(X + subregion.X, Y + subregion.Y, subregion.Width, subregion.Height);
        }

        public static RectangleF Empty => new();

        public static RectangleF FromLTRB(float left, float top, float right, float bottom) =>
            new(left, top, right - left, bottom - top);
    }
}
