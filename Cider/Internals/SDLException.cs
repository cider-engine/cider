using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Internals
{
    public class SDLException : Exception
    {
        public SDLException(string message) : base(message)
        {}
    }
}
