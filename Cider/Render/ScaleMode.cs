using SDL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Render
{
    public enum ScaleMode
    {
        Invalid = SDL_ScaleMode.SDL_SCALEMODE_INVALID,
        Nearest = SDL_ScaleMode.SDL_SCALEMODE_NEAREST,
        Linear = SDL_ScaleMode.SDL_SCALEMODE_LINEAR,
        PixelArt = SDL_ScaleMode.SDL_SCALEMODE_PIXELART
    }
}
