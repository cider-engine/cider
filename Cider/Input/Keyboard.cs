using Cider.Extensions;
using SDL;
using System;
using System.ComponentModel;

namespace Cider.Input
{
    public static class Keyboard
    {
        private static readonly SDLBool[] _lastState;
        private static readonly SDLBool[] _lastPhysicsState;
        private static readonly unsafe SDLBool* _statePtr;
        private static readonly int _stateNum;

        public static bool IsKeyboardAvailable => SDL3.SDL_HasKeyboard();

        /// <summary>
        /// 返回当前的键盘状态
        /// </summary>
        /// <returns></returns>
        public static unsafe KeyboardState GetState()
        {
            return new(new(_statePtr, _stateNum));
        }

        /// <summary>
        /// <para>返回上一帧的键盘状态</para>
        /// <para>如果当前在物理帧中，返回上一物理帧的键盘状态</para>
        /// <para>如果当前不在物理帧中，返回上一渲染帧的键盘状态</para>
        /// </summary>
        /// <returns></returns>
        public static KeyboardState GetLastState()
        {
            return Game.Instance.IsInPhysicsFrame ? new(_lastPhysicsState) : new(_lastState);
        }

        public static unsafe Window? FocusedWindow => SDL3.SDL_GetKeyboardFocus() is not null and var ptr ? SDL3.SDL_GetWindowID(ptr).RelativeWindow : null;

        internal static unsafe void Update()
        {
            new Span<SDLBool>(_statePtr, _stateNum).CopyTo(_lastState);
        }

        internal static unsafe void FixedUpdate()
        {
            new Span<SDLBool>(_statePtr, _stateNum).CopyTo(_lastPhysicsState);
        }

        public static bool IsPressed(KeyCode key)
        {
            return GetState().IsDown(key);
        }

        public static bool IsReleased(KeyCode key)
        {
            return GetState().IsUp(key);
        }

        public static bool IsJustPressed(KeyCode key)
        {
            return GetLastState().IsUp(key) && GetState().IsDown(key);
        }

        public static bool IsJustReleased(KeyCode key)
        {
            return GetLastState().IsDown(key) && GetState().IsUp(key);
        }

        static unsafe Keyboard()
        {
            fixed (int* numPtr = &_stateNum)
                _statePtr = SDL3.SDL_GetKeyboardState(numPtr);

            _lastState = new SDLBool[_stateNum];
            _lastPhysicsState = new SDLBool[_stateNum];

            Update();
            FixedUpdate();
        }
    }

    public readonly record struct KeyboardEventArgs(GameTimestamp Timestamp,
        KeyCode Code,
        KeySymbol Symbol,
        KeyModifier Modifier,
        ushort Raw,
        bool IsDown,
        bool IsRepeat);

    public readonly record struct KeyboardId(uint Id)
    {
        public readonly bool IsInvalid => Id == 0;
    }

    public readonly ref struct KeyboardState
    {
        private readonly Span<SDLBool> states;

        internal KeyboardState(Span<SDLBool> states)
        {
            this.states = states;
        }

        public readonly bool this[KeyCode key] => states[(int)key];

        public bool IsUp(KeyCode key) => !states[(int)key];

        public bool IsDown(KeyCode key) => states[(int)key];
    }

    public static class KeyExtensions
    {
        public static bool IsFromKeyCode(this KeySymbol keySymbol) => ((SDL_Keycode)keySymbol & SDL_Keycode.SDLK_SCANCODE_MASK) != 0;

        public static bool IsExtended(this KeySymbol keySymbol) => ((SDL_Keycode)keySymbol & SDL_Keycode.SDLK_EXTENDED_MASK) != 0;
    }

    public enum KeyCode
    {
        Unknown = SDL_Scancode.SDL_SCANCODE_UNKNOWN,

        A = SDL_Scancode.SDL_SCANCODE_A,
        B = SDL_Scancode.SDL_SCANCODE_B,
        C = SDL_Scancode.SDL_SCANCODE_C,
        D = SDL_Scancode.SDL_SCANCODE_D,
        E = SDL_Scancode.SDL_SCANCODE_E,
        F = SDL_Scancode.SDL_SCANCODE_F,
        G = SDL_Scancode.SDL_SCANCODE_G,
        H = SDL_Scancode.SDL_SCANCODE_H,
        I = SDL_Scancode.SDL_SCANCODE_I,
        J = SDL_Scancode.SDL_SCANCODE_J,
        K = SDL_Scancode.SDL_SCANCODE_K,
        L = SDL_Scancode.SDL_SCANCODE_L,
        M = SDL_Scancode.SDL_SCANCODE_M,
        N = SDL_Scancode.SDL_SCANCODE_N,
        O = SDL_Scancode.SDL_SCANCODE_O,
        P = SDL_Scancode.SDL_SCANCODE_P,
        Q = SDL_Scancode.SDL_SCANCODE_Q,
        R = SDL_Scancode.SDL_SCANCODE_R,
        S = SDL_Scancode.SDL_SCANCODE_S,
        T = SDL_Scancode.SDL_SCANCODE_T,
        U = SDL_Scancode.SDL_SCANCODE_U,
        V = SDL_Scancode.SDL_SCANCODE_V,
        W = SDL_Scancode.SDL_SCANCODE_W,
        X = SDL_Scancode.SDL_SCANCODE_X,
        Y = SDL_Scancode.SDL_SCANCODE_Y,
        Z = SDL_Scancode.SDL_SCANCODE_Z,

        Num1 = SDL_Scancode.SDL_SCANCODE_1,
        Num2 = SDL_Scancode.SDL_SCANCODE_2,
        Num3 = SDL_Scancode.SDL_SCANCODE_3,
        Num4 = SDL_Scancode.SDL_SCANCODE_4,
        Num5 = SDL_Scancode.SDL_SCANCODE_5,
        Num6 = SDL_Scancode.SDL_SCANCODE_6,
        Num7 = SDL_Scancode.SDL_SCANCODE_7,
        Num8 = SDL_Scancode.SDL_SCANCODE_8,
        Num9 = SDL_Scancode.SDL_SCANCODE_9,
        Num0 = SDL_Scancode.SDL_SCANCODE_0,

        Return = SDL_Scancode.SDL_SCANCODE_RETURN,
        Escape = SDL_Scancode.SDL_SCANCODE_ESCAPE,
        Backspace = SDL_Scancode.SDL_SCANCODE_BACKSPACE,
        Tab = SDL_Scancode.SDL_SCANCODE_TAB,
        Space = SDL_Scancode.SDL_SCANCODE_SPACE,

        Minus = SDL_Scancode.SDL_SCANCODE_MINUS,
        Equals = SDL_Scancode.SDL_SCANCODE_EQUALS,
        LeftBracket = SDL_Scancode.SDL_SCANCODE_LEFTBRACKET,
        RightBracket = SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET,
        Backslash = SDL_Scancode.SDL_SCANCODE_BACKSLASH,
        NonUsHash = SDL_Scancode.SDL_SCANCODE_NONUSHASH,
        Semicolon = SDL_Scancode.SDL_SCANCODE_SEMICOLON,
        Apostrophe = SDL_Scancode.SDL_SCANCODE_APOSTROPHE,
        Grave = SDL_Scancode.SDL_SCANCODE_GRAVE,
        Comma = SDL_Scancode.SDL_SCANCODE_COMMA,
        Period = SDL_Scancode.SDL_SCANCODE_PERIOD,
        Slash = SDL_Scancode.SDL_SCANCODE_SLASH,
        CapsLock = SDL_Scancode.SDL_SCANCODE_CAPSLOCK,

        F1 = SDL_Scancode.SDL_SCANCODE_F1,
        F2 = SDL_Scancode.SDL_SCANCODE_F2,
        F3 = SDL_Scancode.SDL_SCANCODE_F3,
        F4 = SDL_Scancode.SDL_SCANCODE_F4,
        F5 = SDL_Scancode.SDL_SCANCODE_F5,
        F6 = SDL_Scancode.SDL_SCANCODE_F6,
        F7 = SDL_Scancode.SDL_SCANCODE_F7,
        F8 = SDL_Scancode.SDL_SCANCODE_F8,
        F9 = SDL_Scancode.SDL_SCANCODE_F9,
        F10 = SDL_Scancode.SDL_SCANCODE_F10,
        F11 = SDL_Scancode.SDL_SCANCODE_F11,
        F12 = SDL_Scancode.SDL_SCANCODE_F12,

        PrintScreen = SDL_Scancode.SDL_SCANCODE_PRINTSCREEN,
        ScrollLock = SDL_Scancode.SDL_SCANCODE_SCROLLLOCK,
        Pause = SDL_Scancode.SDL_SCANCODE_PAUSE,
        Insert = SDL_Scancode.SDL_SCANCODE_INSERT,
        Home = SDL_Scancode.SDL_SCANCODE_HOME,
        PageUp = SDL_Scancode.SDL_SCANCODE_PAGEUP,
        Delete = SDL_Scancode.SDL_SCANCODE_DELETE,
        End = SDL_Scancode.SDL_SCANCODE_END,
        PageDown = SDL_Scancode.SDL_SCANCODE_PAGEDOWN,
        Right = SDL_Scancode.SDL_SCANCODE_RIGHT,
        Left = SDL_Scancode.SDL_SCANCODE_LEFT,
        Down = SDL_Scancode.SDL_SCANCODE_DOWN,
        Up = SDL_Scancode.SDL_SCANCODE_UP,

        NumLockClear = SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR,
        KpDivide = SDL_Scancode.SDL_SCANCODE_KP_DIVIDE,
        KpMultiply = SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY,
        KpMinus = SDL_Scancode.SDL_SCANCODE_KP_MINUS,
        KpPlus = SDL_Scancode.SDL_SCANCODE_KP_PLUS,
        KpEnter = SDL_Scancode.SDL_SCANCODE_KP_ENTER,
        Kp1 = SDL_Scancode.SDL_SCANCODE_KP_1,
        Kp2 = SDL_Scancode.SDL_SCANCODE_KP_2,
        Kp3 = SDL_Scancode.SDL_SCANCODE_KP_3,
        Kp4 = SDL_Scancode.SDL_SCANCODE_KP_4,
        Kp5 = SDL_Scancode.SDL_SCANCODE_KP_5,
        Kp6 = SDL_Scancode.SDL_SCANCODE_KP_6,
        Kp7 = SDL_Scancode.SDL_SCANCODE_KP_7,
        Kp8 = SDL_Scancode.SDL_SCANCODE_KP_8,
        Kp9 = SDL_Scancode.SDL_SCANCODE_KP_9,
        Kp0 = SDL_Scancode.SDL_SCANCODE_KP_0,
        KpPeriod = SDL_Scancode.SDL_SCANCODE_KP_PERIOD,

        NonUsBackslash = SDL_Scancode.SDL_SCANCODE_NONUSBACKSLASH,
        Application = SDL_Scancode.SDL_SCANCODE_APPLICATION,
        Power = SDL_Scancode.SDL_SCANCODE_POWER,
        KpEquals = SDL_Scancode.SDL_SCANCODE_KP_EQUALS,
        F13 = SDL_Scancode.SDL_SCANCODE_F13,
        F14 = SDL_Scancode.SDL_SCANCODE_F14,
        F15 = SDL_Scancode.SDL_SCANCODE_F15,
        F16 = SDL_Scancode.SDL_SCANCODE_F16,
        F17 = SDL_Scancode.SDL_SCANCODE_F17,
        F18 = SDL_Scancode.SDL_SCANCODE_F18,
        F19 = SDL_Scancode.SDL_SCANCODE_F19,
        F20 = SDL_Scancode.SDL_SCANCODE_F20,
        F21 = SDL_Scancode.SDL_SCANCODE_F21,
        F22 = SDL_Scancode.SDL_SCANCODE_F22,
        F23 = SDL_Scancode.SDL_SCANCODE_F23,
        F24 = SDL_Scancode.SDL_SCANCODE_F24,
        Execute = SDL_Scancode.SDL_SCANCODE_EXECUTE,
        Help = SDL_Scancode.SDL_SCANCODE_HELP,
        Menu = SDL_Scancode.SDL_SCANCODE_MENU,
        Select = SDL_Scancode.SDL_SCANCODE_SELECT,
        Stop = SDL_Scancode.SDL_SCANCODE_STOP,
        Again = SDL_Scancode.SDL_SCANCODE_AGAIN,
        Undo = SDL_Scancode.SDL_SCANCODE_UNDO,
        Cut = SDL_Scancode.SDL_SCANCODE_CUT,
        Copy = SDL_Scancode.SDL_SCANCODE_COPY,
        Paste = SDL_Scancode.SDL_SCANCODE_PASTE,
        Find = SDL_Scancode.SDL_SCANCODE_FIND,
        Mute = SDL_Scancode.SDL_SCANCODE_MUTE,
        VolumeUp = SDL_Scancode.SDL_SCANCODE_VOLUMEUP,
        VolumeDown = SDL_Scancode.SDL_SCANCODE_VOLUMEDOWN,
        KpComma = SDL_Scancode.SDL_SCANCODE_KP_COMMA,
        KpEqualsAS400 = SDL_Scancode.SDL_SCANCODE_KP_EQUALSAS400,

        International1 = SDL_Scancode.SDL_SCANCODE_INTERNATIONAL1,
        International2 = SDL_Scancode.SDL_SCANCODE_INTERNATIONAL2,
        International3 = SDL_Scancode.SDL_SCANCODE_INTERNATIONAL3,
        International4 = SDL_Scancode.SDL_SCANCODE_INTERNATIONAL4,
        International5 = SDL_Scancode.SDL_SCANCODE_INTERNATIONAL5,
        International6 = SDL_Scancode.SDL_SCANCODE_INTERNATIONAL6,
        International7 = SDL_Scancode.SDL_SCANCODE_INTERNATIONAL7,
        International8 = SDL_Scancode.SDL_SCANCODE_INTERNATIONAL8,
        International9 = SDL_Scancode.SDL_SCANCODE_INTERNATIONAL9,
        Lang1 = SDL_Scancode.SDL_SCANCODE_LANG1,
        Lang2 = SDL_Scancode.SDL_SCANCODE_LANG2,
        Lang3 = SDL_Scancode.SDL_SCANCODE_LANG3,
        Lang4 = SDL_Scancode.SDL_SCANCODE_LANG4,
        Lang5 = SDL_Scancode.SDL_SCANCODE_LANG5,
        Lang6 = SDL_Scancode.SDL_SCANCODE_LANG6,
        Lang7 = SDL_Scancode.SDL_SCANCODE_LANG7,
        Lang8 = SDL_Scancode.SDL_SCANCODE_LANG8,
        Lang9 = SDL_Scancode.SDL_SCANCODE_LANG9,

        AltErase = SDL_Scancode.SDL_SCANCODE_ALTERASE,
        SysReq = SDL_Scancode.SDL_SCANCODE_SYSREQ,
        Cancel = SDL_Scancode.SDL_SCANCODE_CANCEL,
        Clear = SDL_Scancode.SDL_SCANCODE_CLEAR,
        Prior = SDL_Scancode.SDL_SCANCODE_PRIOR,
        Return2 = SDL_Scancode.SDL_SCANCODE_RETURN2,
        Separator = SDL_Scancode.SDL_SCANCODE_SEPARATOR,
        Out = SDL_Scancode.SDL_SCANCODE_OUT,
        Oper = SDL_Scancode.SDL_SCANCODE_OPER,
        ClearAgain = SDL_Scancode.SDL_SCANCODE_CLEARAGAIN,
        CrSel = SDL_Scancode.SDL_SCANCODE_CRSEL,
        ExSel = SDL_Scancode.SDL_SCANCODE_EXSEL,

        Kp00 = SDL_Scancode.SDL_SCANCODE_KP_00,
        Kp000 = SDL_Scancode.SDL_SCANCODE_KP_000,
        ThousandsSeparator = SDL_Scancode.SDL_SCANCODE_THOUSANDSSEPARATOR,
        DecimalSeparator = SDL_Scancode.SDL_SCANCODE_DECIMALSEPARATOR,
        CurrencyUnit = SDL_Scancode.SDL_SCANCODE_CURRENCYUNIT,
        CurrencySubUnit = SDL_Scancode.SDL_SCANCODE_CURRENCYSUBUNIT,
        KpLeftParen = SDL_Scancode.SDL_SCANCODE_KP_LEFTPAREN,
        KpRightParen = SDL_Scancode.SDL_SCANCODE_KP_RIGHTPAREN,
        KpLeftBrace = SDL_Scancode.SDL_SCANCODE_KP_LEFTBRACE,
        KpRightBrace = SDL_Scancode.SDL_SCANCODE_KP_RIGHTBRACE,
        KpTab = SDL_Scancode.SDL_SCANCODE_KP_TAB,
        KpBackspace = SDL_Scancode.SDL_SCANCODE_KP_BACKSPACE,
        KpA = SDL_Scancode.SDL_SCANCODE_KP_A,
        KpB = SDL_Scancode.SDL_SCANCODE_KP_B,
        KpC = SDL_Scancode.SDL_SCANCODE_KP_C,
        KpD = SDL_Scancode.SDL_SCANCODE_KP_D,
        KpE = SDL_Scancode.SDL_SCANCODE_KP_E,
        KpF = SDL_Scancode.SDL_SCANCODE_KP_F,
        KpXor = SDL_Scancode.SDL_SCANCODE_KP_XOR,
        KpPower = SDL_Scancode.SDL_SCANCODE_KP_POWER,
        KpPercent = SDL_Scancode.SDL_SCANCODE_KP_PERCENT,
        KpLess = SDL_Scancode.SDL_SCANCODE_KP_LESS,
        KpGreater = SDL_Scancode.SDL_SCANCODE_KP_GREATER,
        KpAmpersand = SDL_Scancode.SDL_SCANCODE_KP_AMPERSAND,
        KpDoubleAmpersand = SDL_Scancode.SDL_SCANCODE_KP_DBLAMPERSAND,
        KpVerticalBar = SDL_Scancode.SDL_SCANCODE_KP_VERTICALBAR,
        KpDoubleVerticalBar = SDL_Scancode.SDL_SCANCODE_KP_DBLVERTICALBAR,
        KpColon = SDL_Scancode.SDL_SCANCODE_KP_COLON,
        KpHash = SDL_Scancode.SDL_SCANCODE_KP_HASH,
        KpSpace = SDL_Scancode.SDL_SCANCODE_KP_SPACE,
        KpAt = SDL_Scancode.SDL_SCANCODE_KP_AT,
        KpExclam = SDL_Scancode.SDL_SCANCODE_KP_EXCLAM,
        KpMemStore = SDL_Scancode.SDL_SCANCODE_KP_MEMSTORE,
        KpMemRecall = SDL_Scancode.SDL_SCANCODE_KP_MEMRECALL,
        KpMemClear = SDL_Scancode.SDL_SCANCODE_KP_MEMCLEAR,
        KpMemAdd = SDL_Scancode.SDL_SCANCODE_KP_MEMADD,
        KpMemSubtract = SDL_Scancode.SDL_SCANCODE_KP_MEMSUBTRACT,
        KpMemMultiply = SDL_Scancode.SDL_SCANCODE_KP_MEMMULTIPLY,
        KpMemDivide = SDL_Scancode.SDL_SCANCODE_KP_MEMDIVIDE,
        KpPlusMinus = SDL_Scancode.SDL_SCANCODE_KP_PLUSMINUS,
        KpClear = SDL_Scancode.SDL_SCANCODE_KP_CLEAR,
        KpClearEntry = SDL_Scancode.SDL_SCANCODE_KP_CLEARENTRY,
        KpBinary = SDL_Scancode.SDL_SCANCODE_KP_BINARY,
        KpOctal = SDL_Scancode.SDL_SCANCODE_KP_OCTAL,
        KpDecimal = SDL_Scancode.SDL_SCANCODE_KP_DECIMAL,
        KpHexadecimal = SDL_Scancode.SDL_SCANCODE_KP_HEXADECIMAL,

        LeftCtrl = SDL_Scancode.SDL_SCANCODE_LCTRL,
        LeftShift = SDL_Scancode.SDL_SCANCODE_LSHIFT,
        LeftAlt = SDL_Scancode.SDL_SCANCODE_LALT,
        LeftGui = SDL_Scancode.SDL_SCANCODE_LGUI,
        RightCtrl = SDL_Scancode.SDL_SCANCODE_RCTRL,
        RightShift = SDL_Scancode.SDL_SCANCODE_RSHIFT,
        RightAlt = SDL_Scancode.SDL_SCANCODE_RALT,
        RightGui = SDL_Scancode.SDL_SCANCODE_RGUI,

        Mode = SDL_Scancode.SDL_SCANCODE_MODE,

        Sleep = SDL_Scancode.SDL_SCANCODE_SLEEP,
        Wake = SDL_Scancode.SDL_SCANCODE_WAKE,

        ChannelIncrement = SDL_Scancode.SDL_SCANCODE_CHANNEL_INCREMENT,
        ChannelDecrement = SDL_Scancode.SDL_SCANCODE_CHANNEL_DECREMENT,

        MediaPlay = SDL_Scancode.SDL_SCANCODE_MEDIA_PLAY,
        MediaPause = SDL_Scancode.SDL_SCANCODE_MEDIA_PAUSE,
        MediaRecord = SDL_Scancode.SDL_SCANCODE_MEDIA_RECORD,
        MediaFastForward = SDL_Scancode.SDL_SCANCODE_MEDIA_FAST_FORWARD,
        MediaRewind = SDL_Scancode.SDL_SCANCODE_MEDIA_REWIND,
        MediaNextTrack = SDL_Scancode.SDL_SCANCODE_MEDIA_NEXT_TRACK,
        MediaPreviousTrack = SDL_Scancode.SDL_SCANCODE_MEDIA_PREVIOUS_TRACK,
        MediaStop = SDL_Scancode.SDL_SCANCODE_MEDIA_STOP,
        MediaEject = SDL_Scancode.SDL_SCANCODE_MEDIA_EJECT,
        MediaPlayPause = SDL_Scancode.SDL_SCANCODE_MEDIA_PLAY_PAUSE,
        MediaSelect = SDL_Scancode.SDL_SCANCODE_MEDIA_SELECT,

        AcNew = SDL_Scancode.SDL_SCANCODE_AC_NEW,
        AcOpen = SDL_Scancode.SDL_SCANCODE_AC_OPEN,
        AcClose = SDL_Scancode.SDL_SCANCODE_AC_CLOSE,
        AcExit = SDL_Scancode.SDL_SCANCODE_AC_EXIT,
        AcSave = SDL_Scancode.SDL_SCANCODE_AC_SAVE,
        AcPrint = SDL_Scancode.SDL_SCANCODE_AC_PRINT,
        AcProperties = SDL_Scancode.SDL_SCANCODE_AC_PROPERTIES,

        AcSearch = SDL_Scancode.SDL_SCANCODE_AC_SEARCH,
        AcHome = SDL_Scancode.SDL_SCANCODE_AC_HOME,
        AcBack = SDL_Scancode.SDL_SCANCODE_AC_BACK,
        AcForward = SDL_Scancode.SDL_SCANCODE_AC_FORWARD,
        AcStop = SDL_Scancode.SDL_SCANCODE_AC_STOP,
        AcRefresh = SDL_Scancode.SDL_SCANCODE_AC_REFRESH,
        AcBookmarks = SDL_Scancode.SDL_SCANCODE_AC_BOOKMARKS,

        SoftLeft = SDL_Scancode.SDL_SCANCODE_SOFTLEFT,
        SoftRight = SDL_Scancode.SDL_SCANCODE_SOFTRIGHT,
        Call = SDL_Scancode.SDL_SCANCODE_CALL,
        EndCall = SDL_Scancode.SDL_SCANCODE_ENDCALL,

        [EditorBrowsable(EditorBrowsableState.Never)]
        Reserved = SDL_Scancode.SDL_SCANCODE_RESERVED,

        [EditorBrowsable(EditorBrowsableState.Never)]
        Count = SDL_Scancode.SDL_SCANCODE_COUNT
    }

    public enum KeySymbol : uint
    {
        Unknown = SDL_Keycode.SDLK_UNKNOWN,
        Return = SDL_Keycode.SDLK_RETURN,
        Escape = SDL_Keycode.SDLK_ESCAPE,
        Backspace = SDL_Keycode.SDLK_BACKSPACE,
        Tab = SDL_Keycode.SDLK_TAB,
        Space = SDL_Keycode.SDLK_SPACE,
        Exclaim = SDL_Keycode.SDLK_EXCLAIM,
        DblApostrophe = SDL_Keycode.SDLK_DBLAPOSTROPHE,
        Hash = SDL_Keycode.SDLK_HASH,
        Dollar = SDL_Keycode.SDLK_DOLLAR,
        Percent = SDL_Keycode.SDLK_PERCENT,
        Ampersand = SDL_Keycode.SDLK_AMPERSAND,
        Apostrophe = SDL_Keycode.SDLK_APOSTROPHE,
        LeftParen = SDL_Keycode.SDLK_LEFTPAREN,
        RightParen = SDL_Keycode.SDLK_RIGHTPAREN,
        Asterisk = SDL_Keycode.SDLK_ASTERISK,
        Plus = SDL_Keycode.SDLK_PLUS,
        Comma = SDL_Keycode.SDLK_COMMA,
        Minus = SDL_Keycode.SDLK_MINUS,
        Period = SDL_Keycode.SDLK_PERIOD,
        Slash = SDL_Keycode.SDLK_SLASH,
        D0 = SDL_Keycode.SDLK_0,
        D1 = SDL_Keycode.SDLK_1,
        D2 = SDL_Keycode.SDLK_2,
        D3 = SDL_Keycode.SDLK_3,
        D4 = SDL_Keycode.SDLK_4,
        D5 = SDL_Keycode.SDLK_5,
        D6 = SDL_Keycode.SDLK_6,
        D7 = SDL_Keycode.SDLK_7,
        D8 = SDL_Keycode.SDLK_8,
        D9 = SDL_Keycode.SDLK_9,
        Colon = SDL_Keycode.SDLK_COLON,
        Semicolon = SDL_Keycode.SDLK_SEMICOLON,
        Less = SDL_Keycode.SDLK_LESS,
        Equals = SDL_Keycode.SDLK_EQUALS,
        Greater = SDL_Keycode.SDLK_GREATER,
        Question = SDL_Keycode.SDLK_QUESTION,
        At = SDL_Keycode.SDLK_AT,
        LeftBracket = SDL_Keycode.SDLK_LEFTBRACKET,
        Backslash = SDL_Keycode.SDLK_BACKSLASH,
        RightBracket = SDL_Keycode.SDLK_RIGHTBRACKET,
        Caret = SDL_Keycode.SDLK_CARET,
        Underscore = SDL_Keycode.SDLK_UNDERSCORE,
        Grave = SDL_Keycode.SDLK_GRAVE,
        A = SDL_Keycode.SDLK_A,
        B = SDL_Keycode.SDLK_B,
        C = SDL_Keycode.SDLK_C,
        D = SDL_Keycode.SDLK_D,
        E = SDL_Keycode.SDLK_E,
        F = SDL_Keycode.SDLK_F,
        G = SDL_Keycode.SDLK_G,
        H = SDL_Keycode.SDLK_H,
        I = SDL_Keycode.SDLK_I,
        J = SDL_Keycode.SDLK_J,
        K = SDL_Keycode.SDLK_K,
        L = SDL_Keycode.SDLK_L,
        M = SDL_Keycode.SDLK_M,
        N = SDL_Keycode.SDLK_N,
        O = SDL_Keycode.SDLK_O,
        P = SDL_Keycode.SDLK_P,
        Q = SDL_Keycode.SDLK_Q,
        R = SDL_Keycode.SDLK_R,
        S = SDL_Keycode.SDLK_S,
        T = SDL_Keycode.SDLK_T,
        U = SDL_Keycode.SDLK_U,
        V = SDL_Keycode.SDLK_V,
        W = SDL_Keycode.SDLK_W,
        X = SDL_Keycode.SDLK_X,
        Y = SDL_Keycode.SDLK_Y,
        Z = SDL_Keycode.SDLK_Z,
        LeftBrace = SDL_Keycode.SDLK_LEFTBRACE,
        Pipe = SDL_Keycode.SDLK_PIPE,
        RightBrace = SDL_Keycode.SDLK_RIGHTBRACE,
        Tilde = SDL_Keycode.SDLK_TILDE,
        Delete = SDL_Keycode.SDLK_DELETE,
        PlusMinus = SDL_Keycode.SDLK_PLUSMINUS,
        CapsLock = SDL_Keycode.SDLK_CAPSLOCK,
        F1 = SDL_Keycode.SDLK_F1,
        F2 = SDL_Keycode.SDLK_F2,
        F3 = SDL_Keycode.SDLK_F3,
        F4 = SDL_Keycode.SDLK_F4,
        F5 = SDL_Keycode.SDLK_F5,
        F6 = SDL_Keycode.SDLK_F6,
        F7 = SDL_Keycode.SDLK_F7,
        F8 = SDL_Keycode.SDLK_F8,
        F9 = SDL_Keycode.SDLK_F9,
        F10 = SDL_Keycode.SDLK_F10,
        F11 = SDL_Keycode.SDLK_F11,
        F12 = SDL_Keycode.SDLK_F12,
        PrintScreen = SDL_Keycode.SDLK_PRINTSCREEN,
        ScrollLock = SDL_Keycode.SDLK_SCROLLLOCK,
        Pause = SDL_Keycode.SDLK_PAUSE,
        Insert = SDL_Keycode.SDLK_INSERT,
        Home = SDL_Keycode.SDLK_HOME,
        PageUp = SDL_Keycode.SDLK_PAGEUP,
        End = SDL_Keycode.SDLK_END,
        PageDown = SDL_Keycode.SDLK_PAGEDOWN,
        Right = SDL_Keycode.SDLK_RIGHT,
        Left = SDL_Keycode.SDLK_LEFT,
        Down = SDL_Keycode.SDLK_DOWN,
        Up = SDL_Keycode.SDLK_UP,
        NumLockClear = SDL_Keycode.SDLK_NUMLOCKCLEAR,
        KpDivide = SDL_Keycode.SDLK_KP_DIVIDE,
        KpMultiply = SDL_Keycode.SDLK_KP_MULTIPLY,
        KpMinus = SDL_Keycode.SDLK_KP_MINUS,
        KpPlus = SDL_Keycode.SDLK_KP_PLUS,
        KpEnter = SDL_Keycode.SDLK_KP_ENTER,
        Kp1 = SDL_Keycode.SDLK_KP_1,
        Kp2 = SDL_Keycode.SDLK_KP_2,
        Kp3 = SDL_Keycode.SDLK_KP_3,
        Kp4 = SDL_Keycode.SDLK_KP_4,
        Kp5 = SDL_Keycode.SDLK_KP_5,
        Kp6 = SDL_Keycode.SDLK_KP_6,
        Kp7 = SDL_Keycode.SDLK_KP_7,
        Kp8 = SDL_Keycode.SDLK_KP_8,
        Kp9 = SDL_Keycode.SDLK_KP_9,
        Kp0 = SDL_Keycode.SDLK_KP_0,
        KpPeriod = SDL_Keycode.SDLK_KP_PERIOD,
        Application = SDL_Keycode.SDLK_APPLICATION,
        Power = SDL_Keycode.SDLK_POWER,
        KpEquals = SDL_Keycode.SDLK_KP_EQUALS,
        F13 = SDL_Keycode.SDLK_F13,
        F14 = SDL_Keycode.SDLK_F14,
        F15 = SDL_Keycode.SDLK_F15,
        F16 = SDL_Keycode.SDLK_F16,
        F17 = SDL_Keycode.SDLK_F17,
        F18 = SDL_Keycode.SDLK_F18,
        F19 = SDL_Keycode.SDLK_F19,
        F20 = SDL_Keycode.SDLK_F20,
        F21 = SDL_Keycode.SDLK_F21,
        F22 = SDL_Keycode.SDLK_F22,
        F23 = SDL_Keycode.SDLK_F23,
        F24 = SDL_Keycode.SDLK_F24,
        Execute = SDL_Keycode.SDLK_EXECUTE,
        Help = SDL_Keycode.SDLK_HELP,
        Menu = SDL_Keycode.SDLK_MENU,
        Select = SDL_Keycode.SDLK_SELECT,
        Stop = SDL_Keycode.SDLK_STOP,
        Again = SDL_Keycode.SDLK_AGAIN,
        Undo = SDL_Keycode.SDLK_UNDO,
        Cut = SDL_Keycode.SDLK_CUT,
        Copy = SDL_Keycode.SDLK_COPY,
        Paste = SDL_Keycode.SDLK_PASTE,
        Find = SDL_Keycode.SDLK_FIND,
        Mute = SDL_Keycode.SDLK_MUTE,
        VolumeUp = SDL_Keycode.SDLK_VOLUMEUP,
        VolumeDown = SDL_Keycode.SDLK_VOLUMEDOWN,
        KpComma = SDL_Keycode.SDLK_KP_COMMA,
        KpEqualsAs400 = SDL_Keycode.SDLK_KP_EQUALSAS400,
        Alterase = SDL_Keycode.SDLK_ALTERASE,
        SysReq = SDL_Keycode.SDLK_SYSREQ,
        Cancel = SDL_Keycode.SDLK_CANCEL,
        Clear = SDL_Keycode.SDLK_CLEAR,
        Prior = SDL_Keycode.SDLK_PRIOR,
        Return2 = SDL_Keycode.SDLK_RETURN2,
        Separator = SDL_Keycode.SDLK_SEPARATOR,
        Out = SDL_Keycode.SDLK_OUT,
        Oper = SDL_Keycode.SDLK_OPER,
        ClearAgain = SDL_Keycode.SDLK_CLEARAGAIN,
        CrSel = SDL_Keycode.SDLK_CRSEL,
        ExSel = SDL_Keycode.SDLK_EXSEL,
        Kp00 = SDL_Keycode.SDLK_KP_00,
        Kp000 = SDL_Keycode.SDLK_KP_000,
        ThousandsSeparator = SDL_Keycode.SDLK_THOUSANDSSEPARATOR,
        DecimalSeparator = SDL_Keycode.SDLK_DECIMALSEPARATOR,
        CurrencyUnit = SDL_Keycode.SDLK_CURRENCYUNIT,
        CurrencySubunit = SDL_Keycode.SDLK_CURRENCYSUBUNIT,
        KpLeftParen = SDL_Keycode.SDLK_KP_LEFTPAREN,
        KpRightParen = SDL_Keycode.SDLK_KP_RIGHTPAREN,
        KpLeftBrace = SDL_Keycode.SDLK_KP_LEFTBRACE,
        KpRightBrace = SDL_Keycode.SDLK_KP_RIGHTBRACE,
        KpTab = SDL_Keycode.SDLK_KP_TAB,
        KpBackspace = SDL_Keycode.SDLK_KP_BACKSPACE,
        KpA = SDL_Keycode.SDLK_KP_A,
        KpB = SDL_Keycode.SDLK_KP_B,
        KpC = SDL_Keycode.SDLK_KP_C,
        KpD = SDL_Keycode.SDLK_KP_D,
        KpE = SDL_Keycode.SDLK_KP_E,
        KpF = SDL_Keycode.SDLK_KP_F,
        KpXor = SDL_Keycode.SDLK_KP_XOR,
        KpPower = SDL_Keycode.SDLK_KP_POWER,
        KpPercent = SDL_Keycode.SDLK_KP_PERCENT,
        KpLess = SDL_Keycode.SDLK_KP_LESS,
        KpGreater = SDL_Keycode.SDLK_KP_GREATER,
        KpAmpersand = SDL_Keycode.SDLK_KP_AMPERSAND,
        KpDblAmpersand = SDL_Keycode.SDLK_KP_DBLAMPERSAND,
        KpVerticalBar = SDL_Keycode.SDLK_KP_VERTICALBAR,
        KpDblVerticalBar = SDL_Keycode.SDLK_KP_DBLVERTICALBAR,
        KpColon = SDL_Keycode.SDLK_KP_COLON,
        KpHash = SDL_Keycode.SDLK_KP_HASH,
        KpSpace = SDL_Keycode.SDLK_KP_SPACE,
        KpAt = SDL_Keycode.SDLK_KP_AT,
        KpExclam = SDL_Keycode.SDLK_KP_EXCLAM,
        KpMemStore = SDL_Keycode.SDLK_KP_MEMSTORE,
        KpMemRecall = SDL_Keycode.SDLK_KP_MEMRECALL,
        KpMemClear = SDL_Keycode.SDLK_KP_MEMCLEAR,
        KpMemAdd = SDL_Keycode.SDLK_KP_MEMADD,
        KpMemSubtract = SDL_Keycode.SDLK_KP_MEMSUBTRACT,
        KpMemMultiply = SDL_Keycode.SDLK_KP_MEMMULTIPLY,
        KpMemDivide = SDL_Keycode.SDLK_KP_MEMDIVIDE,
        KpPlusMinus = SDL_Keycode.SDLK_KP_PLUSMINUS,
        KpClear = SDL_Keycode.SDLK_KP_CLEAR,
        KpClearEntry = SDL_Keycode.SDLK_KP_CLEARENTRY,
        KpBinary = SDL_Keycode.SDLK_KP_BINARY,
        KpOctal = SDL_Keycode.SDLK_KP_OCTAL,
        KpDecimal = SDL_Keycode.SDLK_KP_DECIMAL,
        KpHexadecimal = SDL_Keycode.SDLK_KP_HEXADECIMAL,
        LCtrl = SDL_Keycode.SDLK_LCTRL,
        LShift = SDL_Keycode.SDLK_LSHIFT,
        LAlt = SDL_Keycode.SDLK_LALT,
        LGui = SDL_Keycode.SDLK_LGUI,
        RCtrl = SDL_Keycode.SDLK_RCTRL,
        RShift = SDL_Keycode.SDLK_RSHIFT,
        RAlt = SDL_Keycode.SDLK_RALT,
        RGui = SDL_Keycode.SDLK_RGUI,
        Mode = SDL_Keycode.SDLK_MODE,
        Sleep = SDL_Keycode.SDLK_SLEEP,
        Wake = SDL_Keycode.SDLK_WAKE,
        ChannelIncrement = SDL_Keycode.SDLK_CHANNEL_INCREMENT,
        ChannelDecrement = SDL_Keycode.SDLK_CHANNEL_DECREMENT,
        MediaPlay = SDL_Keycode.SDLK_MEDIA_PLAY,
        MediaPause = SDL_Keycode.SDLK_MEDIA_PAUSE,
        MediaRecord = SDL_Keycode.SDLK_MEDIA_RECORD,
        MediaFastForward = SDL_Keycode.SDLK_MEDIA_FAST_FORWARD,
        MediaRewind = SDL_Keycode.SDLK_MEDIA_REWIND,
        MediaNextTrack = SDL_Keycode.SDLK_MEDIA_NEXT_TRACK,
        MediaPreviousTrack = SDL_Keycode.SDLK_MEDIA_PREVIOUS_TRACK,
        MediaStop = SDL_Keycode.SDLK_MEDIA_STOP,
        MediaEject = SDL_Keycode.SDLK_MEDIA_EJECT,
        MediaPlayPause = SDL_Keycode.SDLK_MEDIA_PLAY_PAUSE,
        MediaSelect = SDL_Keycode.SDLK_MEDIA_SELECT,
        AcNew = SDL_Keycode.SDLK_AC_NEW,
        AcOpen = SDL_Keycode.SDLK_AC_OPEN,
        AcClose = SDL_Keycode.SDLK_AC_CLOSE,
        AcExit = SDL_Keycode.SDLK_AC_EXIT,
        AcSave = SDL_Keycode.SDLK_AC_SAVE,
        AcPrint = SDL_Keycode.SDLK_AC_PRINT,
        AcProperties = SDL_Keycode.SDLK_AC_PROPERTIES,
        AcSearch = SDL_Keycode.SDLK_AC_SEARCH,
        AcHome = SDL_Keycode.SDLK_AC_HOME,
        AcBack = SDL_Keycode.SDLK_AC_BACK,
        AcForward = SDL_Keycode.SDLK_AC_FORWARD,
        AcStop = SDL_Keycode.SDLK_AC_STOP,
        AcRefresh = SDL_Keycode.SDLK_AC_REFRESH,
        AcBookmarks = SDL_Keycode.SDLK_AC_BOOKMARKS,
        SoftLeft = SDL_Keycode.SDLK_SOFTLEFT,
        SoftRight = SDL_Keycode.SDLK_SOFTRIGHT,
        Call = SDL_Keycode.SDLK_CALL,
        EndCall = SDL_Keycode.SDLK_ENDCALL,
        LeftTab = SDL_Keycode.SDLK_LEFT_TAB,
        Level5Shift = SDL_Keycode.SDLK_LEVEL5_SHIFT,
        MultiKeyCompose = SDL_Keycode.SDLK_MULTI_KEY_COMPOSE,
        LMeta = SDL_Keycode.SDLK_LMETA,
        RMeta = SDL_Keycode.SDLK_RMETA,
        LHyper = SDL_Keycode.SDLK_LHYPER,
        RHyper = SDL_Keycode.SDLK_RHYPER,
    }

    [Flags]
    public enum KeyModifier : ushort
    {
        None = SDL_Keymod.SDL_KMOD_NONE,
        LeftShift = SDL_Keymod.SDL_KMOD_LSHIFT,
        RightShift = SDL_Keymod.SDL_KMOD_RSHIFT,
        Level5 = SDL_Keymod.SDL_KMOD_LEVEL5,
        LeftCtrl = SDL_Keymod.SDL_KMOD_LCTRL,
        RightCtrl = SDL_Keymod.SDL_KMOD_RCTRL,
        LeftAlt = SDL_Keymod.SDL_KMOD_LALT,
        RightAlt = SDL_Keymod.SDL_KMOD_RALT,
        LeftGui = SDL_Keymod.SDL_KMOD_LGUI,
        RightGui = SDL_Keymod.SDL_KMOD_RGUI,
        Num = SDL_Keymod.SDL_KMOD_NUM,
        Caps = SDL_Keymod.SDL_KMOD_CAPS,
        Mode = SDL_Keymod.SDL_KMOD_MODE,
        Scroll = SDL_Keymod.SDL_KMOD_SCROLL,
        Ctrl = SDL_Keymod.SDL_KMOD_CTRL,
        Shift = SDL_Keymod.SDL_KMOD_SHIFT,
        Alt = SDL_Keymod.SDL_KMOD_ALT,
        Gui = SDL_Keymod.SDL_KMOD_GUI,
    }
}
