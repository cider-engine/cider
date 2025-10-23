using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class XmlnsDefinitionAttribute(string xmlnsUri, string codeNamespace) : Attribute
    {
        public string XmlnsUri => xmlnsUri;
        public string CodeNamespace => codeNamespace;
    }
}
