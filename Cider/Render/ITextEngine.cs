using SDL;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Cider.Render
{
    public interface ITextEngine : IDisposable
    {
        internal unsafe TTF_TextEngine* Pointer { get; }

        void RenderTo(Text text, float x, float y);
    }
}
