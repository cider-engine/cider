using System;

namespace Cider.Data.In2D
{
    public record struct ColorF
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public ColorF(float red, float green, float blue, float alpha)
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }

        public static implicit operator ColorF(Color color) => new(
            red: color.R / (float)byte.MaxValue,
            green: color.G / (float)byte.MaxValue,
            blue: color.B / (float)byte.MaxValue,
            alpha: color.A / (float)byte.MaxValue
            );

        public static explicit operator Color(ColorF color) => new(
            red: byte.CreateChecked(color.R * byte.MaxValue),
            green: byte.CreateChecked(color.G * byte.MaxValue),
            blue: byte.CreateChecked(color.B * byte.MaxValue),
            alpha: byte.CreateChecked(color.A * byte.MaxValue)
            );
    }
}
