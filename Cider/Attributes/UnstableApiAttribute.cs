using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Attributes
{
#nullable enable
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class UnstableApiAttribute(string? message = default) : Attribute
    {
        public string? Message => message;
    }
}
