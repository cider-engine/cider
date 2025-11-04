using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class XmlnsDirectTypeAttribute : Attribute
    {
        public XmlnsDirectTypeAttribute(string xmlnsUri, Type directType)
        {
            XmlnsUri = xmlnsUri;
            DirectType = directType;
        }
        public string XmlnsUri { get; }
        public Type DirectType { get; }
    }
}
