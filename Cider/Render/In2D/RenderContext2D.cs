using Cider.Data.In2D;
using System;

namespace Cider.Render.In2D
{
    public class RenderContext2D : RenderContext
    {
        public Transform2D CurrentTransform2D { get; internal set; }

        public RenderContext2D ApplyTransform(Transform2D transform)
        {
            CurrentTransform2D = CurrentTransform2D.ApplyTransform2D(transform);
            return this;
        }
    }
}
