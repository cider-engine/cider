using System;
using System.Collections.Generic;
using System.Text;

namespace Cider
{
    public class GameRuntimeException : CiderGameException
    {
        public GameRuntimeException(string message) : base(message)
        {}
    }
}
