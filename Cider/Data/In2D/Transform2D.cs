using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Data.In2D
{
    public struct Transform2D
    {
        public Transform2D()
        {
            Position = Vector2.Zero;
            RotationInRadians = 0;
            Scale = Vector2.One;
        }

        public Transform2D(Vector2 relativePosition, float rotationInRadians, Vector2 scale)
        {
            Position = relativePosition;
            RotationInRadians = rotationInRadians;
            Scale = scale;
        }

        public Vector2 Position { readonly get; set; }

        public float RotationInRadians { readonly get; set; }

        public float RotationInDegrees
        {
            readonly get => RotationInRadians * (180 / MathF.PI);
            set => RotationInRadians = value * (MathF.PI / 180);
        }

        public Vector2 Scale { readonly get; set; }

        public readonly Transform2D ApplyTransform2D(Transform2D transform)
        {
            return new(Position + transform.Position,
                RotationInRadians + transform.RotationInRadians,
                Scale * transform.Scale);
        }
    }
}
