using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Data
{
    public enum Keys
    {
        //
        // 摘要:
        //     Reserved.
        None = 0,
        //
        // 摘要:
        //     BACKSPACE key.
        Back = 8,
        //
        // 摘要:
        //     TAB key.
        Tab = 9,
        //
        // 摘要:
        //     ENTER key.
        Enter = 13,
        //
        // 摘要:
        //     CAPS LOCK key.
        CapsLock = 20,
        //
        // 摘要:
        //     ESC key.
        Escape = 27,
        //
        // 摘要:
        //     SPACEBAR key.
        Space = 32,
        //
        // 摘要:
        //     PAGE UP key.
        PageUp = 33,
        //
        // 摘要:
        //     PAGE DOWN key.
        PageDown = 34,
        //
        // 摘要:
        //     END key.
        End = 35,
        //
        // 摘要:
        //     HOME key.
        Home = 36,
        //
        // 摘要:
        //     LEFT ARROW key.
        Left = 37,
        //
        // 摘要:
        //     UP ARROW key.
        Up = 38,
        //
        // 摘要:
        //     RIGHT ARROW key.
        Right = 39,
        //
        // 摘要:
        //     DOWN ARROW key.
        Down = 40,
        //
        // 摘要:
        //     SELECT key.
        Select = 41,
        //
        // 摘要:
        //     PRINT key.
        Print = 42,
        //
        // 摘要:
        //     EXECUTE key.
        Execute = 43,
        //
        // 摘要:
        //     PRINT SCREEN key.
        PrintScreen = 44,
        //
        // 摘要:
        //     INS key.
        Insert = 45,
        //
        // 摘要:
        //     DEL key.
        Delete = 46,
        //
        // 摘要:
        //     HELP key.
        Help = 47,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        D0 = 48,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        D1 = 49,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        D2 = 50,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        D3 = 51,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        D4 = 52,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        D5 = 53,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        D6 = 54,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        D7 = 55,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        D8 = 56,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        D9 = 57,
        //
        // 摘要:
        //     A key.
        A = 65,
        //
        // 摘要:
        //     B key.
        B = 66,
        //
        // 摘要:
        //     C key.
        C = 67,
        //
        // 摘要:
        //     D key.
        D = 68,
        //
        // 摘要:
        //     E key.
        E = 69,
        //
        // 摘要:
        //     F key.
        F = 70,
        //
        // 摘要:
        //     G key.
        G = 71,
        //
        // 摘要:
        //     H key.
        H = 72,
        //
        // 摘要:
        //     I key.
        I = 73,
        //
        // 摘要:
        //     J key.
        J = 74,
        //
        // 摘要:
        //     K key.
        K = 75,
        //
        // 摘要:
        //     L key.
        L = 76,
        //
        // 摘要:
        //     M key.
        M = 77,
        //
        // 摘要:
        //     N key.
        N = 78,
        //
        // 摘要:
        //     O key.
        O = 79,
        //
        // 摘要:
        //     P key.
        P = 80,
        //
        // 摘要:
        //     Q key.
        Q = 81,
        //
        // 摘要:
        //     R key.
        R = 82,
        //
        // 摘要:
        //     S key.
        S = 83,
        //
        // 摘要:
        //     T key.
        T = 84,
        //
        // 摘要:
        //     U key.
        U = 85,
        //
        // 摘要:
        //     V key.
        V = 86,
        //
        // 摘要:
        //     W key.
        W = 87,
        //
        // 摘要:
        //     X key.
        X = 88,
        //
        // 摘要:
        //     Y key.
        Y = 89,
        //
        // 摘要:
        //     Z key.
        Z = 90,
        //
        // 摘要:
        //     Left Windows key.
        LeftWindows = 91,
        //
        // 摘要:
        //     Right Windows key.
        RightWindows = 92,
        //
        // 摘要:
        //     Applications key.
        Apps = 93,
        //
        // 摘要:
        //     Computer Sleep key.
        Sleep = 95,
        //
        // 摘要:
        //     Numeric keypad 0 key.
        NumPad0 = 96,
        //
        // 摘要:
        //     Numeric keypad 1 key.
        NumPad1 = 97,
        //
        // 摘要:
        //     Numeric keypad 2 key.
        NumPad2 = 98,
        //
        // 摘要:
        //     Numeric keypad 3 key.
        NumPad3 = 99,
        //
        // 摘要:
        //     Numeric keypad 4 key.
        NumPad4 = 100,
        //
        // 摘要:
        //     Numeric keypad 5 key.
        NumPad5 = 101,
        //
        // 摘要:
        //     Numeric keypad 6 key.
        NumPad6 = 102,
        //
        // 摘要:
        //     Numeric keypad 7 key.
        NumPad7 = 103,
        //
        // 摘要:
        //     Numeric keypad 8 key.
        NumPad8 = 104,
        //
        // 摘要:
        //     Numeric keypad 9 key.
        NumPad9 = 105,
        //
        // 摘要:
        //     Multiply key.
        Multiply = 106,
        //
        // 摘要:
        //     Add key.
        Add = 107,
        //
        // 摘要:
        //     Separator key.
        Separator = 108,
        //
        // 摘要:
        //     Subtract key.
        Subtract = 109,
        //
        // 摘要:
        //     Decimal key.
        Decimal = 110,
        //
        // 摘要:
        //     Divide key.
        Divide = 111,
        //
        // 摘要:
        //     F1 key.
        F1 = 112,
        //
        // 摘要:
        //     F2 key.
        F2 = 113,
        //
        // 摘要:
        //     F3 key.
        F3 = 114,
        //
        // 摘要:
        //     F4 key.
        F4 = 115,
        //
        // 摘要:
        //     F5 key.
        F5 = 116,
        //
        // 摘要:
        //     F6 key.
        F6 = 117,
        //
        // 摘要:
        //     F7 key.
        F7 = 118,
        //
        // 摘要:
        //     F8 key.
        F8 = 119,
        //
        // 摘要:
        //     F9 key.
        F9 = 120,
        //
        // 摘要:
        //     F10 key.
        F10 = 121,
        //
        // 摘要:
        //     F11 key.
        F11 = 122,
        //
        // 摘要:
        //     F12 key.
        F12 = 123,
        //
        // 摘要:
        //     F13 key.
        F13 = 124,
        //
        // 摘要:
        //     F14 key.
        F14 = 125,
        //
        // 摘要:
        //     F15 key.
        F15 = 126,
        //
        // 摘要:
        //     F16 key.
        F16 = 127,
        //
        // 摘要:
        //     F17 key.
        F17 = 128,
        //
        // 摘要:
        //     F18 key.
        F18 = 129,
        //
        // 摘要:
        //     F19 key.
        F19 = 130,
        //
        // 摘要:
        //     F20 key.
        F20 = 131,
        //
        // 摘要:
        //     F21 key.
        F21 = 132,
        //
        // 摘要:
        //     F22 key.
        F22 = 133,
        //
        // 摘要:
        //     F23 key.
        F23 = 134,
        //
        // 摘要:
        //     F24 key.
        F24 = 135,
        //
        // 摘要:
        //     NUM LOCK key.
        NumLock = 144,
        //
        // 摘要:
        //     SCROLL LOCK key.
        Scroll = 145,
        //
        // 摘要:
        //     Left SHIFT key.
        LeftShift = 160,
        //
        // 摘要:
        //     Right SHIFT key.
        RightShift = 161,
        //
        // 摘要:
        //     Left CONTROL key.
        LeftControl = 162,
        //
        // 摘要:
        //     Right CONTROL key.
        RightControl = 163,
        //
        // 摘要:
        //     Left ALT key.
        LeftAlt = 164,
        //
        // 摘要:
        //     Right ALT key.
        RightAlt = 165,
        //
        // 摘要:
        //     Browser Back key.
        BrowserBack = 166,
        //
        // 摘要:
        //     Browser Forward key.
        BrowserForward = 167,
        //
        // 摘要:
        //     Browser Refresh key.
        BrowserRefresh = 168,
        //
        // 摘要:
        //     Browser Stop key.
        BrowserStop = 169,
        //
        // 摘要:
        //     Browser Search key.
        BrowserSearch = 170,
        //
        // 摘要:
        //     Browser Favorites key.
        BrowserFavorites = 171,
        //
        // 摘要:
        //     Browser Start and Home key.
        BrowserHome = 172,
        //
        // 摘要:
        //     Volume Mute key.
        VolumeMute = 173,
        //
        // 摘要:
        //     Volume Down key.
        VolumeDown = 174,
        //
        // 摘要:
        //     Volume Up key.
        VolumeUp = 175,
        //
        // 摘要:
        //     Next Track key.
        MediaNextTrack = 176,
        //
        // 摘要:
        //     Previous Track key.
        MediaPreviousTrack = 177,
        //
        // 摘要:
        //     Stop Media key.
        MediaStop = 178,
        //
        // 摘要:
        //     Play/Pause Media key.
        MediaPlayPause = 179,
        //
        // 摘要:
        //     Start Mail key.
        LaunchMail = 180,
        //
        // 摘要:
        //     Select Media key.
        SelectMedia = 181,
        //
        // 摘要:
        //     Start Application 1 key.
        LaunchApplication1 = 182,
        //
        // 摘要:
        //     Start Application 2 key.
        LaunchApplication2 = 183,
        //
        // 摘要:
        //     The OEM Semicolon key on a US standard keyboard.
        OemSemicolon = 186,
        //
        // 摘要:
        //     For any country/region, the '+' key.
        OemPlus = 187,
        //
        // 摘要:
        //     For any country/region, the ',' key.
        OemComma = 188,
        //
        // 摘要:
        //     For any country/region, the '-' key.
        OemMinus = 189,
        //
        // 摘要:
        //     For any country/region, the '.' key.
        OemPeriod = 190,
        //
        // 摘要:
        //     The OEM question mark key on a US standard keyboard.
        OemQuestion = 191,
        //
        // 摘要:
        //     The OEM tilde key on a US standard keyboard.
        OemTilde = 192,
        //
        // 摘要:
        //     The OEM open bracket key on a US standard keyboard.
        OemOpenBrackets = 219,
        //
        // 摘要:
        //     The OEM pipe key on a US standard keyboard.
        OemPipe = 220,
        //
        // 摘要:
        //     The OEM close bracket key on a US standard keyboard.
        OemCloseBrackets = 221,
        //
        // 摘要:
        //     The OEM singled/double quote key on a US standard keyboard.
        OemQuotes = 222,
        //
        // 摘要:
        //     Used for miscellaneous characters; it can vary by keyboard.
        Oem8 = 223,
        //
        // 摘要:
        //     The OEM angle bracket or backslash key on the RT 102 key keyboard.
        OemBackslash = 226,
        //
        // 摘要:
        //     IME PROCESS key.
        ProcessKey = 229,
        //
        // 摘要:
        //     Attn key.
        Attn = 246,
        //
        // 摘要:
        //     CrSel key.
        Crsel = 247,
        //
        // 摘要:
        //     ExSel key.
        Exsel = 248,
        //
        // 摘要:
        //     Erase EOF key.
        EraseEof = 249,
        //
        // 摘要:
        //     Play key.
        Play = 250,
        //
        // 摘要:
        //     Zoom key.
        Zoom = 251,
        //
        // 摘要:
        //     PA1 key.
        Pa1 = 253,
        //
        // 摘要:
        //     CLEAR key.
        OemClear = 254,
        //
        // 摘要:
        //     Green ChatPad key.
        ChatPadGreen = 202,
        //
        // 摘要:
        //     Orange ChatPad key.
        ChatPadOrange = 203,
        //
        // 摘要:
        //     PAUSE key.
        Pause = 19,
        //
        // 摘要:
        //     IME Convert key.
        ImeConvert = 28,
        //
        // 摘要:
        //     IME NoConvert key.
        ImeNoConvert = 29,
        //
        // 摘要:
        //     Kana key on Japanese keyboards.
        Kana = 21,
        //
        // 摘要:
        //     Kanji key on Japanese keyboards.
        Kanji = 25,
        //
        // 摘要:
        //     OEM Auto key.
        OemAuto = 243,
        //
        // 摘要:
        //     OEM Copy key.
        OemCopy = 242,
        //
        // 摘要:
        //     OEM Enlarge Window key.
        OemEnlW = 244
    }
}
