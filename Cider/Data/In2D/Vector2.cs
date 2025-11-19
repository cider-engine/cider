using System;
using System.Runtime.CompilerServices;

namespace Cider.Data.In2D
{
    public struct Vector2 : IEquatable<Vector2>
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float LengthSquared() => X * X + Y * Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Length() => MathF.Sqrt(LengthSquared());
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector2 Add(float x, float y) => new(x + X, y + Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector2 Subtract(float x, float y) => new(X - x, Y - y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector2 Rotate(float angleInRadians)
        {
            float cos = MathF.Cos(angleInRadians);
            float sin = MathF.Sin(angleInRadians);
            return new Vector2(X * cos - Y * sin, X * sin + Y * cos);
        }

        public readonly override string ToString() => $"Vector2({X}, {Y})";

        public readonly override bool Equals(object obj)
        {
            return obj is Vector2 vector && X == vector.X && Y == vector.Y;
        }

        public readonly bool Equals(Vector2 other)
        {
            return X == other.X && Y == other.Y;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator -(Vector2 vector) => new(-vector.X, -vector.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator +(Vector2 left, Vector2 right) => new(left.X + right.X, left.Y + right.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator -(Vector2 left, Vector2 right) => new(left.X - right.X, left.Y - right.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(Vector2 vector, float multiplier) => new(vector.X * multiplier, vector.Y * multiplier);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator /(Vector2 vector, float multiplier) => new(vector.X / multiplier, vector.Y / multiplier);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(float multiplier, Vector2 vector) => new(vector.X * multiplier, vector.Y * multiplier);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(Vector2 left, Vector2 right) => new(left.X * right.X, left.Y * right.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector2 left, Vector2 right) => left.X == right.X && left.Y == right.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector2 left, Vector2 right) => left.X != right.X || left.Y != right.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Deconstruct(out float x, out float y)
        {
            x = X;
            y = Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 FromPoint(Microsoft.Xna.Framework.Point point) => new(point.X, point.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector2((float X, float Y) tuple) => new(tuple.X, tuple.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector2(Microsoft.Xna.Framework.Vector2 vector) => new(vector.X, vector.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Microsoft.Xna.Framework.Vector2(Vector2 vector) => new(vector.X, vector.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector2(nkast.Aether.Physics2D.Common.Vector2 vector) => new(vector.X, vector.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator nkast.Aether.Physics2D.Common.Vector2(Vector2 vector) => new(vector.X, vector.Y);
    }
}
