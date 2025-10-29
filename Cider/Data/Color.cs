using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Data
{
    public struct Color
    {
        public Color()
        {
            A = 255;
        }

        public Color(byte red, byte green, byte blue)
        {
            R = red;
            G = green;
            B = blue;
            A = 255;
        }

        public Color(byte red, byte green, byte blue, byte alpha)
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }

        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public static implicit operator Microsoft.Xna.Framework.Color (Color c) => new(c.R, c.G, c.B, c.A);

        //
        // 摘要:
        //     Transparent color (R:0,G:0,B:0,A:0).
        public static Color Transparent => new(0, 0, 0, 0);

        //
        // 摘要:
        //     AliceBlue color (R:240,G:248,B:255,A:255).
        public static Color AliceBlue => new(240, 248, 255);

        //
        // 摘要:
        //     AntiqueWhite color (R:250,G:235,B:215,A:255).
        public static Color AntiqueWhite => new(250, 235, 215);

        //
        // 摘要:
        //     Aqua color (R:0,G:255,B:255,A:255).
        public static Color Aqua => new(0, 255, 255);

        //
        // 摘要:
        //     Aquamarine color (R:127,G:255,B:212,A:255).
        public static Color Aquamarine => new(127, 255, 212);

        //
        // 摘要:
        //     Azure color (R:240,G:255,B:255,A:255).
        public static Color Azure => new(240, 255, 255);

        //
        // 摘要:
        //     Beige color (R:245,G:245,B:220,A:255).
        public static Color Beige => new(245, 245, 220);

        //
        // 摘要:
        //     Bisque color (R:255,G:228,B:196,A:255).
        public static Color Bisque => new(255, 228, 196);

        //
        // 摘要:
        //     Black color (R:0,G:0,B:0,A:255).
        public static Color Black => new(0, 0, 0);

        //
        // 摘要:
        //     BlanchedAlmond color (R:255,G:235,B:205,A:255).
        public static Color BlanchedAlmond => new(255, 235, 205);

        //
        // 摘要:
        //     Blue color (R:0,G:0,B:255,A:255).
        public static Color Blue => new(0, 0, 255);

        //
        // 摘要:
        //     BlueViolet color (R:138,G:43,B:226,A:255).
        public static Color BlueViolet => new(138, 43, 226);

        //
        // 摘要:
        //     Brown color (R:165,G:42,B:42,A:255).
        public static Color Brown => new(165, 42, 42);

        //
        // 摘要:
        //     BurlyWood color (R:222,G:184,B:135,A:255).
        public static Color BurlyWood => new(222, 184, 135);

        //
        // 摘要:
        //     CadetBlue color (R:95,G:158,B:160,A:255).
        public static Color CadetBlue => new(95, 158, 160);

        //
        // 摘要:
        //     Chartreuse color (R:127,G:255,B:0,A:255).
        public static Color Chartreuse => new(127, 255, 0);

        //
        // 摘要:
        //     Chocolate color (R:210,G:105,B:30,A:255).
        public static Color Chocolate => new(210, 105, 30);

        //
        // 摘要:
        //     Coral color (R:255,G:127,B:80,A:255).
        public static Color Coral => new(255, 127, 80);

        //
        // 摘要:
        //     CornflowerBlue color (R:100,G:149,B:237,A:255).
        public static Color CornflowerBlue => new(100, 149, 237);

        //
        // 摘要:
        //     Cornsilk color (R:255,G:248,B:220,A:255).
        public static Color Cornsilk => new(255, 248, 220);

        //
        // 摘要:
        //     Crimson color (R:220,G:20,B:60,A:255).
        public static Color Crimson => new(220, 20, 60);

        //
        // 摘要:
        //     Cyan color (R:0,G:255,B:255,A:255).
        public static Color Cyan => new(0, 255, 255);

        //
        // 摘要:
        //     DarkBlue color (R:0,G:0,B:139,A:255).
        public static Color DarkBlue => new(0, 0, 139);

        //
        // 摘要:
        //     DarkCyan color (R:0,G:139,B:139,A:255).
        public static Color DarkCyan => new(0, 139, 139);

        //
        // 摘要:
        //     DarkGoldenrod color (R:184,G:134,B:11,A:255).
        public static Color DarkGoldenrod => new(184, 134, 11);

        //                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  
        // 摘要:
        //     DarkGray color (R:169,G:169,B:169,A:255).
        public static Color DarkGray => new(169, 169, 169);

        //
        // 摘要:
        //     DarkGreen color (R:0,G:100,B:0,A:255).
        public static Color DarkGreen => new(0, 100, 0);

        //
        // 摘要:
        //     DarkKhaki color (R:189,G:183,B:107,A:255).
        public static Color DarkKhaki => new(189, 183, 107);

        //
        // 摘要:
        //     DarkMagenta color (R:139,G:0,B:139,A:255).
        public static Color DarkMagenta => new(139, 0, 139);

        //
        // 摘要:
        //     DarkOliveGreen color (R:85,G:107,B:47,A:255).
        public static Color DarkOliveGreen => new(85, 107, 47);

        //
        // 摘要:
        //     DarkOrange color (R:255,G:140,B:0,A:255).
        public static Color DarkOrange => new(255, 140, 0);

        //
        // 摘要:
        //     DarkOrchid color (R:153,G:50,B:204,A:255).
        public static Color DarkOrchid => new(153, 50, 204);

        //
        // 摘要:
        //     DarkRed color (R:139,G:0,B:0,A:255).
        public static Color DarkRed => new(139, 0, 0);

        //
        // 摘要:
        //     DarkSalmon color (R:233,G:150,B:122,A:255).
        public static Color DarkSalmon => new(233, 150, 122);

        //
        // 摘要:
        //     DarkSeaGreen color (R:143,G:188,B:139,A:255).
        public static Color DarkSeaGreen => new(143, 188, 139);

        //
        // 摘要:
        //     DarkSlateBlue color (R:72,G:61,B:139,A:255).
        public static Color DarkSlateBlue => new(72, 61, 139);

        //
        // 摘要:
        //     DarkSlateGray color (R:47,G:79,B:79,A:255).
        public static Color DarkSlateGray => new(47, 79, 79);

        //
        // 摘要:
        //     DarkTurquoise color (R:0,G:206,B:209,A:255).
        public static Color DarkTurquoise => new(0, 206, 209);

        //
        // 摘要:
        //     DarkViolet color (R:148,G:0,B:211,A:255).
        public static Color DarkViolet => new(148, 0, 211);

        //
        // 摘要:
        //     DeepPink color (R:255,G:20,B:147,A:255).
        public static Color DeepPink => new(255, 20, 147);

        //
        // 摘要:
        //     DeepSkyBlue color (R:0,G:191,B:255,A:255).
        public static Color DeepSkyBlue => new(0, 191, 255);

        //
        // 摘要:
        //     DimGray color (R:105,G:105,B:105,A:255).
        public static Color DimGray => new(105, 105, 105);

        //
        // 摘要:
        //     DodgerBlue color (R:30,G:144,B:255,A:255).
        public static Color DodgerBlue => new(30, 144, 255);

        //
        // 摘要:
        //     Firebrick color (R:178,G:34,B:34,A:255).
        public static Color Firebrick => new(178, 34, 34);

        //
        // 摘要:
        //     FloralWhite color (R:255,G:250,B:240,A:255).
        public static Color FloralWhite => new(255, 250, 240);

        //
        // 摘要:
        //     ForestGreen color (R:34,G:139,B:34,A:255).
        public static Color ForestGreen => new(34, 139, 34);

        //
        // 摘要:
        //     Fuchsia color (R:255,G:0,B:255,A:255).
        public static Color Fuchsia => new(255, 0, 255);

        //
        // 摘要:
        //     Gainsboro color (R:220,G:220,B:220,A:255).
        public static Color Gainsboro => new(220, 220, 220);

        //
        // 摘要:
        //     GhostWhite color (R:248,G:248,B:255,A:255).
        public static Color GhostWhite => new(248, 248, 255);

        //
        // 摘要:
        //     Gold color (R:255,G:215,B:0,A:255).
        public static Color Gold => new(255, 215, 0);

        //
        // 摘要:
        //     Goldenrod color (R:218,G:165,B:32,A:255).
        public static Color Goldenrod => new(218, 165, 32);

        //
        // 摘要:
        //     Gray color (R:128,G:128,B:128,A:255).
        public static Color Gray => new(128, 128, 128);

        //
        // 摘要:
        //     Green color (R:0,G:128,B:0,A:255).
        public static Color Green => new(0, 128, 0);

        //
        // 摘要:
        //     GreenYellow color (R:173,G:255,B:47,A:255).
        public static Color GreenYellow => new(173, 255, 47);

        //
        // 摘要:
        //     Honeydew color (R:240,G:255,B:240,A:255).
        public static Color Honeydew => new(240, 255, 240);

        //
        // 摘要:
        //     HotPink color (R:255,G:105,B:180,A:255).
        public static Color HotPink => new(255, 105, 180);

        //
        // 摘要:
        //     IndianRed color (R:205,G:92,B:92,A:255).
        public static Color IndianRed => new(205, 92, 92);

        //
        // 摘要:
        //     Indigo color (R:75,G:0,B:130,A:255).
        public static Color Indigo => new(75, 0, 130);

        //
        // 摘要:
        //     Ivory color (R:255,G:255,B:240,A:255).
        public static Color Ivory => new(255, 255, 240);

        //
        // 摘要:
        //     Khaki color (R:240,G:230,B:140,A:255).
        public static Color Khaki => new(240, 230, 140);

        //
        // 摘要:
        //     Lavender color (R:230,G:230,B:250,A:255).
        public static Color Lavender => new(230, 230, 250);

        //
        // 摘要:
        //     LavenderBlush color (R:255,G:240,B:245,A:255).
        public static Color LavenderBlush => new(255, 240, 245);

        //
        // 摘要:
        //     LawnGreen color (R:124,G:252,B:0,A:255).
        public static Color LawnGreen => new(124, 252, 0);

        //
        // 摘要:
        //     LemonChiffon color (R:255,G:250,B:205,A:255).
        public static Color LemonChiffon => new(255, 250, 205);

        //
        // 摘要:
        //     LightBlue color (R:173,G:216,B:230,A:255).
        public static Color LightBlue => new(173, 216, 230);

        //
        // 摘要:
        //     LightCoral color (R:240,G:128,B:128,A:255).
        public static Color LightCoral => new(240, 128, 128);

        //
        // 摘要:
        //     LightCyan color (R:224,G:255,B:255,A:255).
        public static Color LightCyan => new(224, 255, 255);

        //
        // 摘要:
        //     LightGoldenrodYellow color (R:250,G:250,B:210,A:255).
        public static Color LightGoldenrodYellow => new(250, 250, 210);

        //
        // 摘要:
        //     LightGray color (R:211,G:211,B:211,A:255).
        public static Color LightGray => new(211, 211, 211);

        //
        // 摘要:
        //     LightGreen color (R:144,G:238,B:144,A:255).
        public static Color LightGreen => new(144, 238, 144);

        //
        // 摘要:
        //     LightPink color (R:255,G:182,B:193,A:255).
        public static Color LightPink => new(255, 182, 193);

        //
        // 摘要:
        //     LightSalmon color (R:255,G:160,B:122,A:255).
        public static Color LightSalmon => new(255, 160, 122);

        //
        // 摘要:
        //     LightSeaGreen color (R:32,G:178,B:170,A:255).
        public static Color LightSeaGreen => new(32, 178, 170);

        //
        // 摘要:
        //     LightSkyBlue color (R:135,G:206,B:250,A:255).
        public static Color LightSkyBlue => new(135, 206, 250);

        //
        // 摘要:
        //     LightSlateGray color (R:119,G:136,B:153,A:255).
        public static Color LightSlateGray => new(119, 136, 153);

        //
        // 摘要:
        //     LightSteelBlue color (R:176,G:196,B:222,A:255).
        public static Color LightSteelBlue => new(176, 196, 222);

        //
        // 摘要:
        //     LightYellow color (R:255,G:255,B:224,A:255).
        public static Color LightYellow => new(255, 255, 224);

        //
        // 摘要:
        //     Lime color (R:0,G:255,B:0,A:255).
        public static Color Lime => new(0, 255, 0);

        //
        // 摘要:
        //     LimeGreen color (R:50,G:205,B:50,A:255).
        public static Color LimeGreen => new(50, 205, 50);

        //
        // 摘要:
        //     Linen color (R:250,G:240,B:230,A:255).
        public static Color Linen => new(250, 240, 230);

        //
        // 摘要:
        //     Magenta color (R:255,G:0,B:255,A:255).
        public static Color Magenta => new(255, 0, 255);

        //
        // 摘要:
        //     Maroon color (R:128,G:0,B:0,A:255).
        public static Color Maroon => new(128, 0, 0);

        //
        // 摘要:
        //     MediumAquamarine color (R:102,G:205,B:170,A:255).
        public static Color MediumAquamarine => new(102, 205, 170);

        //
        // 摘要:
        //     MediumBlue color (R:0,G:0,B:205,A:255).
        public static Color MediumBlue => new(0, 0, 205);

        //
        // 摘要:
        //     MediumOrchid color (R:186,G:85,B:211,A:255).
        public static Color MediumOrchid => new(186, 85, 211);

        //
        // 摘要:
        //     MediumPurple color (R:147,G:112,B:219,A:255).
        public static Color MediumPurple => new(147, 112, 219);

        //
        // 摘要:
        //     MediumSeaGreen color (R:60,G:179,B:113,A:255).
        public static Color MediumSeaGreen => new(60, 179, 113);

        //
        // 摘要:
        //     MediumSlateBlue color (R:123,G:104,B:238,A:255).
        public static Color MediumSlateBlue => new(123, 104, 238);

        //
        // 摘要:
        //     MediumSpringGreen color (R:0,G:250,B:154,A:255).
        public static Color MediumSpringGreen => new(0, 250, 154);

        //
        // 摘要:
        //     MediumTurquoise color (R:72,G:209,B:204,A:255).
        public static Color MediumTurquoise => new(72, 209, 204);

        //
        // 摘要:
        //     MediumVioletRed color (R:199,G:21,B:133,A:255).
        public static Color MediumVioletRed => new(199, 21, 133);

        //
        // 摘要:
        //     MidnightBlue color (R:25,G:25,B:112,A:255).
        public static Color MidnightBlue => new(25, 25, 112);

        //
        // 摘要:
        //     MintCream color (R:245,G:255,B:250,A:255).
        public static Color MintCream => new(245, 255, 250);

        //
        // 摘要:
        //     MistyRose color (R:255,G:228,B:225,A:255).
        public static Color MistyRose => new(255, 228, 225);

        //
        // 摘要:
        //     Moccasin color (R:255,G:228,B:181,A:255).
        public static Color Moccasin => new(255, 228, 181);

        //
        // 摘要:
        //     MonoGame orange theme color (R:231,G:60,B:0,A:255).
        public static Color MonoGameOrange => new(231, 60, 0);

        //
        // 摘要:
        //     NavajoWhite color (R:255,G:222,B:173,A:255).
        public static Color NavajoWhite => new(255, 222, 173);

        //
        // 摘要:
        //     Navy color (R:0,G:0,B:128,A:255).
        public static Color Navy => new(0, 0, 128);

        //
        // 摘要:
        //     OldLace color (R:253,G:245,B:230,A:255).
        public static Color OldLace => new(253, 245, 230);

        //
        // 摘要:
        //     Olive color (R:128,G:128,B:0,A:255).
        public static Color Olive => new(128, 128, 0);

        //
        // 摘要:
        //     OliveDrab color (R:107,G:142,B:35,A:255).
        public static Color OliveDrab => new(107, 142, 35);

        //
        // 摘要:
        //     Orange color (R:255,G:165,B:0,A:255).
        public static Color Orange => new(255, 165, 0);

        //
        // 摘要:
        //     OrangeRed color (R:255,G:69,B:0,A:255).
        public static Color OrangeRed => new(255, 69, 0);

        //
        // 摘要:
        //     Orchid color (R:218,G:112,B:214,A:255).
        public static Color Orchid => new(218, 112, 214);

        //
        // 摘要:
        //     PaleGoldenrod color (R:238,G:232,B:170,A:255).
        public static Color PaleGoldenrod => new(238, 232, 170);

        //
        // 摘要:
        //     PaleGreen color (R:152,G:251,B:152,A:255).
        public static Color PaleGreen => new(152, 251, 152);

        //
        // 摘要:
        //     PaleTurquoise color (R:175,G:238,B:238,A:255).
        public static Color PaleTurquoise => new(175, 238, 238);

        //
        // 摘要:
        //     PaleVioletRed color (R:219,G:112,B:147,A:255).
        public static Color PaleVioletRed => new(219, 112, 147);

        //
        // 摘要:
        //     PapayaWhip color (R:255,G:239,B:213,A:255).
        public static Color PapayaWhip => new(255, 239, 213);

        //
        // 摘要:
        //     PeachPuff color (R:255,G:218,B:185,A:255).
        public static Color PeachPuff => new(255, 218, 185);

        //
        // 摘要:
        //     Peru color (R:205,G:133,B:63,A:255).
        public static Color Peru => new(205, 133, 63);

        //
        // 摘要:
        //     Pink color (R:255,G:192,B:203,A:255).
        public static Color Pink => new(255, 192, 203);

        //
        // 摘要:
        //     Plum color (R:221,G:160,B:221,A:255).
        public static Color Plum => new(221, 160, 221);

        //
        // 摘要:
        //     PowderBlue color (R:176,G:224,B:230,A:255).
        public static Color PowderBlue => new(176, 224, 230);

        //
        // 摘要:
        //     Purple color (R:128,G:0,B:128,A:255).
        public static Color Purple => new(128, 0, 128);

        //
        // 摘要:
        //     Red color (R:255,G:0,B:0,A:255).
        public static Color Red => new(255, 0, 0);

        //
        // 摘要:
        //     RosyBrown color (R:188,G:143,B:143,A:255).
        public static Color RosyBrown => new(188, 143, 143);

        //
        // 摘要:
        //     RoyalBlue color (R:65,G:105,B:225,A:255).
        public static Color RoyalBlue => new(65, 105, 225);

        //
        // 摘要:
        //     SaddleBrown color (R:139,G:69,B:19,A:255).
        public static Color SaddleBrown => new(139, 69, 19);

        //
        // 摘要:
        //     Salmon color (R:250,G:128,B:114,A:255).
        public static Color Salmon => new(250, 128, 114);

        //
        // 摘要:
        //     SandyBrown color (R:244,G:164,B:96,A:255).
        public static Color SandyBrown => new(244, 164, 96);

        //
        // 摘要:
        //     SeaGreen color (R:46,G:139,B:87,A:255).
        public static Color SeaGreen => new(46, 139, 87);

        //
        // 摘要:
        //     SeaShell color (R:255,G:245,B:238,A:255).
        public static Color SeaShell => new(255, 245, 238);

        //
        // 摘要:
        //     Sienna color (R:160,G:82,B:45,A:255).
        public static Color Sienna => new(160, 82, 45);

        //
        // 摘要:
        //     Silver color (R:192,G:192,B:192,A:255).
        public static Color Silver => new(192, 192, 192);

        //
        // 摘要:
        //     SkyBlue color (R:135,G:206,B:235,A:255).
        public static Color SkyBlue => new(135, 206, 235);

        //
        // 摘要:
        //     SlateBlue color (R:106,G:90,B:205,A:255).
        public static Color SlateBlue => new(106, 90, 205);

        //
        // 摘要:
        //     SlateGray color (R:112,G:128,B:144,A:255).
        public static Color SlateGray => new(112, 128, 144);

        //
        // 摘要:
        //     Snow color (R:255,G:250,B:250,A:255).
        public static Color Snow => new(255, 250, 250);

        //
        // 摘要:
        //     SpringGreen color (R:0,G:255,B:127,A:255).
        public static Color SpringGreen => new(0, 255, 127);

        //
        // 摘要:
        //     SteelBlue color (R:70,G:130,B:180,A:255).
        public static Color SteelBlue => new(70, 130, 180);

        //
        // 摘要:
        //     Tan color (R:210,G:180,B:140,A:255).
        public static Color Tan => new(210, 180, 140);

        //
        // 摘要:
        //     Teal color (R:0,G:128,B:128,A:255).
        public static Color Teal => new(0, 128, 128);

        //
        // 摘要:
        //     Thistle color (R:216,G:191,B:216,A:255).
        public static Color Thistle => new(216, 191, 216);

        //
        // 摘要:
        //     Tomato color (R:255,G:99,B:71,A:255).
        public static Color Tomato => new(255, 99, 71);

        //
        // 摘要:
        //     Turquoise color (R:64,G:224,B:208,A:255).
        public static Color Turquoise => new(64, 224, 208);

        //
        // 摘要:
        //     Violet color (R:238,G:130,B:238,A:255).
        public static Color Violet => new(238, 130, 238);

        //
        // 摘要:
        //     Wheat color (R:245,G:222,B:179,A:255).
        public static Color Wheat => new(245, 222, 179);

        //
        // 摘要:
        //     White color (R:255,G:255,B:255,A:255).
        public static Color White => new(255, 255, 255);

        //
        // 摘要:
        //     WhiteSmoke color (R:245,G:245,B:245,A:255).
        public static Color WhiteSmoke => new(245, 245, 245);

        //
        // 摘要:
        //     Yellow color (R:255,G:255,B:0,A:255).
        public static Color Yellow => new(255, 255, 0);

        //
        // 摘要:
        //     YellowGreen color (R:154,G:205,B:50,A:255).
        public static Color YellowGreen => new(154, 205, 50);
    }
}
