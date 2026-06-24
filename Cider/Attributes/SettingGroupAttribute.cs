using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingGroupAttribute(string path) : Attribute
    {
        public string Path => path;
    }
}
