using SDL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Render
{
    public enum LogicalPresentationMode
    {
        Disabled = SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_DISABLED,
        Stretch = SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_STRETCH,
        Letterbox = SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_LETTERBOX,
        Overscan = SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_OVERSCAN,
        IntegerScale = SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_INTEGER_SCALE
    }
}
