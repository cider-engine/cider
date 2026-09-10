using System;

namespace Cider.Data.In2D
{
    public record struct Color
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color(byte red, byte green, byte blue, byte alpha)
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }

        public Color(byte red, byte green, byte blue)
        {
            R = red;
            G = green;
            B = blue;
            A = byte.MaxValue;
        }

        public Color(uint rgba)
        {
            R = (byte)((rgba >> 24) & 0xFF);
            G = (byte)((rgba >> 16) & 0xFF);
            B = (byte)((rgba >> 8) & 0xFF);
            A = (byte)(rgba & 0xFF);
        }

        public static Color Black => new(0x000000FF);
        public static Color White => new(0xFFFFFFFF);
        public static Color Transparent => new(0x00000000);
        public static Color Red => new(0xFF0000FF);
        public static Color Green => new(0x008000FF);
        public static Color Blue => new(0x0000FFFF);
        public static Color Yellow => new(0xFFFF00FF);
        public static Color Cyan => new(0x00FFFFFF);
        public static Color Magenta => new(0xFF00FFFF);
        public static Color Silver => new(0xC0C0C0FF);
        public static Color Gray => new(0x808080FF);
        public static Color Maroon => new(0x800000FF);
        public static Color Olive => new(0x808000FF);
        public static Color Purple => new(0x800080FF);
        public static Color Teal => new(0x008080FF);
        public static Color Navy => new(0x000080FF);

        public static Color AliceBlue => new(0xF0F8FFFF);
        public static Color AntiqueWhite => new(0xFAEBD7FF);
        public static Color Aqua => new(0x00FFFFFF);
        public static Color Aquamarine => new(0x7FFFD4FF);
        public static Color Azure => new(0xF0FFFFFF);
        public static Color Beige => new(0xF5F5DCFF);
        public static Color Bisque => new(0xFFE4C4FF);
        public static Color BlanchedAlmond => new(0xFFEBCDFF);
        public static Color BlueViolet => new(0x8A2BE2FF);
        public static Color Brown => new(0xA52A2AFF);
        public static Color BurlyWood => new(0xDEB887FF);
        public static Color CadetBlue => new(0x5F9EA0FF);
        public static Color Chartreuse => new(0x7FFF00FF);
        public static Color Chocolate => new(0xD2691EFF);
        public static Color Coral => new(0xFF7F50FF);
        public static Color CornflowerBlue => new(0x6495EDFF);
        public static Color Cornsilk => new(0xFFF8DCFF);
        public static Color Crimson => new(0xDC143CFF);
        public static Color DarkBlue => new(0x00008BFF);
        public static Color DarkCyan => new(0x008B8BFF);
        public static Color DarkGoldenrod => new(0xB8860BFF);
        public static Color DarkGray => new(0xA9A9A9FF);
        public static Color DarkGreen => new(0x006400FF);
        public static Color DarkKhaki => new(0xBDB76BFF);
        public static Color DarkMagenta => new(0x8B008BFF);
        public static Color DarkOliveGreen => new(0x556B2FFF);
        public static Color DarkOrange => new(0xFF8C00FF);
        public static Color DarkOrchid => new(0x9932CCFF);
        public static Color DarkRed => new(0x8B0000FF);
        public static Color DarkSalmon => new(0xE9967AFF);
        public static Color DarkSeaGreen => new(0x8FBC8FFF);
        public static Color DarkSlateBlue => new(0x483D8BFF);
        public static Color DarkSlateGray => new(0x2F4F4FFF);
        public static Color DarkTurquoise => new(0x00CED1FF);
        public static Color DarkViolet => new(0x9400D3FF);
        public static Color DeepPink => new(0xFF1493FF);
        public static Color DeepSkyBlue => new(0x00BFFFFF);
        public static Color DimGray => new(0x696969FF);
        public static Color DodgerBlue => new(0x1E90FFFF);
        public static Color Firebrick => new(0xB22222FF);
        public static Color FloralWhite => new(0xFFFAF0FF);
        public static Color ForestGreen => new(0x228B22FF);
        public static Color Fuchsia => new(0xFF00FFFF);
        public static Color Gainsboro => new(0xDCDCDCFF);
        public static Color GhostWhite => new(0xF8F8FFFF);
        public static Color Gold => new(0xFFD700FF);
        public static Color Goldenrod => new(0xDAA520FF);
        public static Color GreenYellow => new(0xADFF2FFF);
        public static Color Honeydew => new(0xF0FFF0FF);
        public static Color HotPink => new(0xFF69B4FF);
        public static Color IndianRed => new(0xCD5C5CFF);
        public static Color Indigo => new(0x4B0082FF);
        public static Color Ivory => new(0xFFFFF0FF);
        public static Color Khaki => new(0xF0E68CFF);
        public static Color Lavender => new(0xE6E6FAFF);
        public static Color LavenderBlush => new(0xFFF0F5FF);
        public static Color LawnGreen => new(0x7CFC00FF);
        public static Color LemonChiffon => new(0xFFFACDFF);
        public static Color LightBlue => new(0xADD8E6FF);
        public static Color LightCoral => new(0xF08080FF);
        public static Color LightCyan => new(0xE0FFFFFF);
        public static Color LightGoldenrodYellow => new(0xFAFAD2FF);
        public static Color LightGray => new(0xD3D3D3FF);
        public static Color LightGreen => new(0x90EE90FF);
        public static Color LightPink => new(0xFFB6C1FF);
        public static Color LightSalmon => new(0xFFA07AFF);
        public static Color LightSeaGreen => new(0x20B2AAFF);
        public static Color LightSkyBlue => new(0x87CEFAFF);
        public static Color LightSlateGray => new(0x778899FF);
        public static Color LightSteelBlue => new(0xB0C4DEFF);
        public static Color LightYellow => new(0xFFFFE0FF);
        public static Color Lime => new(0x00FF00FF);
        public static Color LimeGreen => new(0x32CD32FF);
        public static Color Linen => new(0xFAF0E6FF);
        public static Color MediumAquamarine => new(0x66CDAAFF);
        public static Color MediumBlue => new(0x0000CDFF);
        public static Color MediumOrchid => new(0xBA55D3FF);
        public static Color MediumPurple => new(0x9370DBFF);
        public static Color MediumSeaGreen => new(0x3CB371FF);
        public static Color MediumSlateBlue => new(0x7B68EEFF);
        public static Color MediumSpringGreen => new(0x00FA9AFF);
        public static Color MediumTurquoise => new(0x48D1CCFF);
        public static Color MediumVioletRed => new(0xC71585FF);
        public static Color MidnightBlue => new(0x191970FF);
        public static Color MintCream => new(0xF5FFFAFF);
        public static Color MistyRose => new(0xFFE4E1FF);
        public static Color Moccasin => new(0xFFE4B5FF);
        public static Color NavajoWhite => new(0xFFDEADFF);
        public static Color OldLace => new(0xFDF5E6FF);
        public static Color OliveDrab => new(0x6B8E23FF);
        public static Color Orange => new(0xFFA500FF);
        public static Color OrangeRed => new(0xFF4500FF);
        public static Color Orchid => new(0xDA70D6FF);
        public static Color PaleGoldenrod => new(0xEEE8AAFF);
        public static Color PaleGreen => new(0x98FB98FF);
        public static Color PaleTurquoise => new(0xAFEEEEFF);
        public static Color PaleVioletRed => new(0xDB7093FF);
        public static Color PapayaWhip => new(0xFFEFD5FF);
        public static Color PeachPuff => new(0xFFDAB9FF);
        public static Color Peru => new(0xCD853FFF);
        public static Color Pink => new(0xFFC0CBFF);
        public static Color Plum => new(0xDDA0DDFF);
        public static Color PowderBlue => new(0xB0E0E6FF);
        public static Color RosyBrown => new(0xBC8F8FFF);
        public static Color RoyalBlue => new(0x4169E1FF);
        public static Color SaddleBrown => new(0x8B4513FF);
        public static Color Salmon => new(0xFA8072FF);
        public static Color SandyBrown => new(0xF4A460FF);
        public static Color SeaGreen => new(0x2E8B57FF);
        public static Color Seashell => new(0xFFF5EEFF);
        public static Color Sienna => new(0xA0522DFF);
        public static Color SkyBlue => new(0x87CEEBFF);
        public static Color SlateBlue => new(0x6A5ACDFF);
        public static Color SlateGray => new(0x708090FF);
        public static Color Snow => new(0xFFFAFAFF);
        public static Color SpringGreen => new(0x00FF7FFF);
        public static Color SteelBlue => new(0x4682B4FF);
        public static Color Tan => new(0xD2B48CFF);
        public static Color Thistle => new(0xD8BFD8FF);
        public static Color Tomato => new(0xFF6347FF);
        public static Color Turquoise => new(0x40E0D0FF);
        public static Color Violet => new(0xEE82EEFF);
        public static Color Wheat => new(0xF5DEB3FF);
        public static Color WhiteSmoke => new(0xF5F5F5FF);
        public static Color YellowGreen => new(0x9ACD32FF);

        public static Color DarkGrey => DarkGray;
        public static Color DimGrey => DimGray;
        public static Color Grey => Gray;
        public static Color LightGrey => LightGray;
        public static Color SlateGrey => SlateGray;
        public static Color DarkSlateGrey => DarkSlateGray;
        public static Color LightSlateGrey => LightSlateGray;

        public static Color FromName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Color name cannot be null or empty", nameof(name));

            return name.ToLowerInvariant() switch
            {
                "black" => Black,
                "white" => White,
                "transparent" => Transparent,
                "red" => Red,
                "green" => Green,
                "blue" => Blue,
                "yellow" => Yellow,
                "cyan" => Cyan,
                "magenta" => Magenta,
                "silver" => Silver,
                "gray" => Gray,
                "grey" => Gray,
                "maroon" => Maroon,
                "olive" => Olive,
                "purple" => Purple,
                "teal" => Teal,
                "navy" => Navy,
                "aliceblue" => AliceBlue,
                "antiquewhite" => AntiqueWhite,
                "aqua" => Aqua,
                "aquamarine" => Aquamarine,
                "azure" => Azure,
                "beige" => Beige,
                "bisque" => Bisque,
                "blanchedalmond" => BlanchedAlmond,
                "blueviolet" => BlueViolet,
                "brown" => Brown,
                "burlywood" => BurlyWood,
                "cadetblue" => CadetBlue,
                "chartreuse" => Chartreuse,
                "chocolate" => Chocolate,
                "coral" => Coral,
                "cornflowerblue" => CornflowerBlue,
                "cornsilk" => Cornsilk,
                "crimson" => Crimson,
                "darkblue" => DarkBlue,
                "darkcyan" => DarkCyan,
                "darkgoldenrod" => DarkGoldenrod,
                "darkgray" => DarkGray,
                "darkgrey" => DarkGray,
                "darkgreen" => DarkGreen,
                "darkkhaki" => DarkKhaki,
                "darkmagenta" => DarkMagenta,
                "darkolivegreen" => DarkOliveGreen,
                "darkorange" => DarkOrange,
                "darkorchid" => DarkOrchid,
                "darkred" => DarkRed,
                "darksalmon" => DarkSalmon,
                "darkseagreen" => DarkSeaGreen,
                "darkslateblue" => DarkSlateBlue,
                "darkslategray" => DarkSlateGray,
                "darkslategrey" => DarkSlateGray,
                "darkturquoise" => DarkTurquoise,
                "darkviolet" => DarkViolet,
                "deeppink" => DeepPink,
                "deepskyblue" => DeepSkyBlue,
                "dimgray" => DimGray,
                "dimgrey" => DimGray,
                "dodgerblue" => DodgerBlue,
                "firebrick" => Firebrick,
                "floralwhite" => FloralWhite,
                "forestgreen" => ForestGreen,
                "fuchsia" => Fuchsia,
                "gainsboro" => Gainsboro,
                "ghostwhite" => GhostWhite,
                "gold" => Gold,
                "goldenrod" => Goldenrod,
                "greenyellow" => GreenYellow,
                "honeydew" => Honeydew,
                "hotpink" => HotPink,
                "indianred" => IndianRed,
                "indigo" => Indigo,
                "ivory" => Ivory,
                "khaki" => Khaki,
                "lavender" => Lavender,
                "lavenderblush" => LavenderBlush,
                "lawngreen" => LawnGreen,
                "lemonchiffon" => LemonChiffon,
                "lightblue" => LightBlue,
                "lightcoral" => LightCoral,
                "lightcyan" => LightCyan,
                "lightgoldenrodyellow" => LightGoldenrodYellow,
                "lightgray" => LightGray,
                "lightgrey" => LightGray,
                "lightgreen" => LightGreen,
                "lightpink" => LightPink,
                "lightsalmon" => LightSalmon,
                "lightseagreen" => LightSeaGreen,
                "lightskyblue" => LightSkyBlue,
                "lightslategray" => LightSlateGray,
                "lightslategrey" => LightSlateGray,
                "lightsteelblue" => LightSteelBlue,
                "lightyellow" => LightYellow,
                "lime" => Lime,
                "limegreen" => LimeGreen,
                "linen" => Linen,
                "mediumaquamarine" => MediumAquamarine,
                "mediumblue" => MediumBlue,
                "mediumorchid" => MediumOrchid,
                "mediumpurple" => MediumPurple,
                "mediumseagreen" => MediumSeaGreen,
                "mediumslateblue" => MediumSlateBlue,
                "mediumspringgreen" => MediumSpringGreen,
                "mediumturquoise" => MediumTurquoise,
                "mediumvioletred" => MediumVioletRed,
                "midnightblue" => MidnightBlue,
                "mintcream" => MintCream,
                "mistyrose" => MistyRose,
                "moccasin" => Moccasin,
                "navajowhite" => NavajoWhite,
                "oldlace" => OldLace,
                "olivedrab" => OliveDrab,
                "orange" => Orange,
                "orangered" => OrangeRed,
                "orchid" => Orchid,
                "palegoldenrod" => PaleGoldenrod,
                "palegreen" => PaleGreen,
                "paleturquoise" => PaleTurquoise,
                "palevioletred" => PaleVioletRed,
                "papayawhip" => PapayaWhip,
                "peachpuff" => PeachPuff,
                "peru" => Peru,
                "pink" => Pink,
                "plum" => Plum,
                "powderblue" => PowderBlue,
                "rosybrown" => RosyBrown,
                "royalblue" => RoyalBlue,
                "saddlebrown" => SaddleBrown,
                "salmon" => Salmon,
                "sandybrown" => SandyBrown,
                "seagreen" => SeaGreen,
                "seashell" => Seashell,
                "sienna" => Sienna,
                "skyblue" => SkyBlue,
                "slateblue" => SlateBlue,
                "slategray" => SlateGray,
                "slategrey" => SlateGray,
                "snow" => Snow,
                "springgreen" => SpringGreen,
                "steelblue" => SteelBlue,
                "tan" => Tan,
                "thistle" => Thistle,
                "tomato" => Tomato,
                "turquoise" => Turquoise,
                "violet" => Violet,
                "wheat" => Wheat,
                "whitesmoke" => WhiteSmoke,
                "yellowgreen" => YellowGreen,
                _ => throw new ArgumentException($"Unknown color name: {name}", nameof(name)),
            };
        }
    }
}
