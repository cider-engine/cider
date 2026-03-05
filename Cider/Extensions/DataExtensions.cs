using SDL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Cider.Extensions
{
    public static class DataExtensions
    {
        extension(Vector2 vector)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Vector2 Rotate(float angleInRadians)
            {
                float cos = MathF.Cos(angleInRadians);
                float sin = MathF.Sin(angleInRadians);
                return new Vector2(vector.X * cos - vector.Y * sin, vector.X * sin + vector.Y * cos);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public nkast.Aether.Physics2D.Common.Vector2 AsPhysicsVector2() => new(vector.X, vector.Y);
        }

        extension(nkast.Aether.Physics2D.Common.Vector2 vector)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Vector2 AsVector2() => new(vector.X, vector.Y);
        }

        extension(RectangleF rect)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal SDL_FRect AsRect() => new()
            {
                x = rect.X,
                y = rect.Y,
                w = rect.Width,
                h = rect.Height
            };
        }

        extension(Color color)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal SDL_Color AsColor() => new()
            {
                r = color.R,
                g = color.G,
                b = color.B,
                a = color.A
            };
        }
    }
}
