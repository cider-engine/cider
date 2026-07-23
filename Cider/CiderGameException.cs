using System;
using System.Collections.Generic;
using System.Text;

#nullable enable
namespace Cider
{
    public class CiderGameException : Exception
    {
        public CiderGameException() : base()
        { }
        public CiderGameException(string? message) : base(message)
        { }
        public CiderGameException(string? message, Exception? innerException) : base(message, innerException)
        { }
    }
}
