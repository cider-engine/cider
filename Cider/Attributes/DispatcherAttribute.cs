using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class DispatcherAttribute : Attribute
    {
    }
}
