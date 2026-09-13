using Cider.Data.In2D;
using System;
using System.Numerics;

namespace Cider.Render
{
    public record struct TextureMappingVertex
    {
        public Vector2 Position;
        public ColorF Color;
        public Vector2 MappingUv;
    }
}
