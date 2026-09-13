using System;

namespace Cider.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CameraIndependentAttribute : Attribute
    {
    }
}