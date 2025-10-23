using System;

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

        public readonly void Deconstruct(out float x, out float y)
        {
            x = X;
            y = Y;
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
