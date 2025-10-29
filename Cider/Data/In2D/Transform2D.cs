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
            // 先按当前 Scale 缩放 transform 的位置（分量相乘），再按当前 Rotation 旋转，最后加上当前 Position
            var scaled = new Vector2(transform.Position.X * Scale.X, transform.Position.Y * Scale.Y);
            var rotated = scaled.Rotate(RotationInRadians);
            return new(Position + rotated,
                RotationInRadians + transform.RotationInRadians,
                Scale * transform.Scale);
        }
    }
}
