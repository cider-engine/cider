using SDL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Render
{
    public enum BlendMode : uint
    {
        None = SDL_BlendMode.SDL_BLENDMODE_NONE,
        Blend = SDL_BlendMode.SDL_BLENDMODE_BLEND,
        BlendPremultiplied = SDL_BlendMode.SDL_BLENDMODE_BLEND_PREMULTIPLIED,
        Add = SDL_BlendMode.SDL_BLENDMODE_ADD,
        AddPremultiplied = SDL_BlendMode.SDL_BLENDMODE_ADD_PREMULTIPLIED,
        Modulate = SDL_BlendMode.SDL_BLENDMODE_MOD,
        Multiply = SDL_BlendMode.SDL_BLENDMODE_MUL,
        Invalid = SDL_BlendMode.SDL_BLENDMODE_INVALID
    }
}
