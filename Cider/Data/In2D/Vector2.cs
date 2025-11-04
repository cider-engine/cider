using System;
using System.Diagnostics.CodeAnalysis;

namespace Cider.Data.In2D
{
    public struct Vector2
    {
        public static Vector2 Zero => new(0, 0);

        public static Vector2 One => new(1, 1);

        public Vector2()
        {}

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X;
        public float Y;

        public readonly Vector2 Add(float x, float y) => new(x + X, y + Y);
        public readonly Vector2 Subtract(float x, float y) => new(X - x, Y - y);

        public readonly Vector2 Rotate(float angleInRadians)
        {
            float cos = MathF.Cos(angleInRadians);
            float sin = MathF.Sin(angleInRadians);
            return new Vector2(X * cos - Y * sin, X * sin + Y * cos);
        }

        public readonly override string ToString() => $"Vector2({X}, {Y})";

        public readonly override bool Equals([NotNullWhen(true)] object obj)
        {
            return obj is Vector2 vector && X == vector.X && Y == vector.Y;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static Vector2 operator -(Vector2 vector)
        {
            return new Vector2
            {
                X = -vector.X,
                Y = -vector.Y
            };
        }

        public static Vector2 operator +(Vector2 left, Vector2 right)
        {
            return new Vector2
            {
                X = left.X + right.X,
                Y = left.Y + right.Y
            };
        }

        public static Vector2 operator -(Vector2 left, Vector2 right)
        {
            return new Vector2
            {
                X = left.X - right.X,
                Y = left.Y - right.Y
            };
        }

        public static Vector2 operator *(Vector2 vector, float multiplier)
        {
            return new Vector2
            {
                X = vector.X * multiplier,
                Y = vector.Y * multiplier
            };
        }

        public static Vector2 operator /(Vector2 vector, float multiplier)
        {
            return new Vector2
            {
                X = vector.X / multiplier,
                Y = vector.Y / multiplier
            };
        }

        public static Vector2 operator *(float multiplier, Vector2 vector)
        {
            return new Vector2
            {
                X = vector.X * multiplier,
                Y = vector.Y * multiplier
            };
        }

        public static Vector2 operator /(float multiplier, Vector2 vector)
        {
            return new Vector2
            {
                X = vector.X / multiplier,
                Y = vector.Y / multiplier
            };
        }

        public static Vector2 operator *(Vector2 left, Vector2 right)
        {
            return new Vector2
            {
                X = left.X * right.X,
                Y = left.Y * right.Y
            };
        }

        public static bool operator ==(Vector2 left, Vector2 right) => left.X == right.X && left.Y == right.Y;

        public static bool operator !=(Vector2 left, Vector2 right) => !(left == right);

        public readonly void Deconstruct(out float x, out float y)
        {
            x = X;
            y = Y;
        }

        public static Vector2 FromPoint(Microsoft.Xna.Framework.Point point)
        {
            return new Vector2 { X = point.X, Y = point.Y };
        }

        public static implicit operator Vector2((float X, float Y) tuple)
        {
            return new Vector2 { X = tuple.X, Y = tuple.Y };
        }

        public static implicit operator Vector2(Microsoft.Xna.Framework.Vector2 vector)
        {
            return new Vector2 { X = vector.X, Y = vector.Y };
        }

        public static implicit operator Microsoft.Xna.Framework.Vector2(Vector2 vector)
        {
            return new Microsoft.Xna.Framework.Vector2 { X = vector.X, Y = vector.Y };
        }

        public static implicit operator Vector2(nkast.Aether.Physics2D.Common.Vector2 vector)
        {
            return new Vector2 { X = vector.X, Y = vector.Y };
        }

        public static implicit operator nkast.Aether.Physics2D.Common.Vector2(Vector2 vector)
        {
            return new nkast.Aether.Physics2D.Common.Vector2 { X = vector.X, Y = vector.Y };
        }
    }
}
