using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Data.In2D
{
    public class Transform2DChangedEventArgs : EventArgs
    {
        public Transform2D CurrentTransform2D { get; internal set; }

        public Transform2DChangedEventArgs ApplyTransform(Transform2D transform)
        {
            CurrentTransform2D = CurrentTransform2D.ApplyTransform2D(transform);
            return this;
        }
    }
}
