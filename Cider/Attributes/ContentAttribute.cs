using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ContentAttribute(string propertyName) : Attribute
    {
        public string PropertyName => propertyName;
    }
}
