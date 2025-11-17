using Cider.Assets;
using Cider.Data;
using Cider.Data.In2D;
using System;
using System.ComponentModel;

namespace Cider.Converters
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly ref partial struct StringValueConverter(string value)
    {
        private readonly string _value = value;

        public static implicit operator string(in StringValueConverter converter) => converter._value;

        public static implicit operator byte(in StringValueConverter converter) => byte.Parse(converter._value);

        public static implicit operator int(in StringValueConverter converter) => int.Parse(converter._value);

        public static implicit operator long(in StringValueConverter converter) => long.Parse(converter._value);

        public static implicit operator float(in StringValueConverter converter) => float.Parse(converter._value);

        public static implicit operator double(in StringValueConverter converter) => double.Parse(converter._value);

        public static implicit operator bool(in StringValueConverter converter) => bool.Parse(converter._value);

        public static implicit operator Vector2(in StringValueConverter converter)
        {
            Span<Range> ranges = stackalloc Range[3];
            if (converter._value.AsSpan().Split(ranges, ',') == 2)
                return new()
                {
                    X = float.Parse(converter._value.AsSpan()[ranges[0]].Trim()),
                    Y = float.Parse(converter._value.AsSpan()[ranges[1]].Trim())
                };

            else
            {
                var value = float.Parse(converter._value);
                return new()
                {
                    X = value,
                    Y = value
                };
            }
        }

        public static implicit operator Color(in StringValueConverter converter)
        {
            if (converter._value.StartsWith('#'))
            {
                var hex = converter._value.AsSpan()[1..];
                if (hex.Length == 6)
                {
                    var r = byte.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber);
                    var g = byte.Parse(hex.Slice(2, 2), System.Globalization.NumberStyles.HexNumber);
                    var b = byte.Parse(hex.Slice(4, 2), System.Globalization.NumberStyles.HexNumber);
                    return new(r, g, b);
                }
                else if (hex.Length == 8)
                {
                    var a = byte.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber);
                    var r = byte.Parse(hex.Slice(2, 2), System.Globalization.NumberStyles.HexNumber);
                    var g = byte.Parse(hex.Slice(4, 2), System.Globalization.NumberStyles.HexNumber);
                    var b = byte.Parse(hex.Slice(6, 2), System.Globalization.NumberStyles.HexNumber);
                    return new(r, g, b, a);
                }
                else
                {
                    throw new FormatException("Invalid color hex format.");
                }
            }
            else
            {
                throw new FormatException("Only hex color format is supported.");
            }
        }
    }
}