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
    }

    public static class Vector2Extensions
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

            public static Vector2 Parse(string value)
            {
                Span<Range> ranges = stackalloc Range[3];
                if (value.AsSpan().Split(ranges, ',') == 2)
                    return new()
                    {
                        X = float.Parse(value.AsSpan()[ranges[0]].Trim()),
                        Y = float.Parse(value.AsSpan()[ranges[1]].Trim())
                    };

                else
                {
                    var xy = float.Parse(value);
                    return new()
                    {
                        X = xy,
                        Y = xy
                    };
                }
            }
        }
    }

    public static class ColorExtensions
    {
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

            public static Color Parse(string value)
            {
                if (value.StartsWith('#'))
                {
                    var hex = value.AsSpan()[1..];

                    if (hex.Length == 6)
                    {
                        var r = byte.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber);
                        var g = byte.Parse(hex.Slice(2, 2), System.Globalization.NumberStyles.HexNumber);
                        var b = byte.Parse(hex.Slice(4, 2), System.Globalization.NumberStyles.HexNumber);
                        return Color.FromArgb(r, g, b);
                    }

                    else if (hex.Length == 8)
                    {
                        var a = byte.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber);
                        var r = byte.Parse(hex.Slice(2, 2), System.Globalization.NumberStyles.HexNumber);
                        var g = byte.Parse(hex.Slice(4, 2), System.Globalization.NumberStyles.HexNumber);
                        var b = byte.Parse(hex.Slice(6, 2), System.Globalization.NumberStyles.HexNumber);
                        return Color.FromArgb(a, r, g, b);
                    }

                    else
                        throw new FormatException("Invalid color hex format.");
                }

                else
                    return Color.FromName(value);
            }
        }
    }
}
