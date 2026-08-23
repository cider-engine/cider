using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class IndirectRenderFunctionAttribute : Attribute
    {
    }
}
