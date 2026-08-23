using System;
using System.Numerics;

namespace Cider.Render
{
    public class Camera2D(Renderer renderer)
    {
        public Renderer OwnerRenderer => renderer;

        public Vector2 OffsetPosition
        {
            get
            {
                if (IsEnabled)
                {
                    if (IsCentered)
                    {
                        var outputSize = renderer.CurrentOutputSize;
                        return new(field.X - outputSize.Width / 2f, field.Y - outputSize.Height / 2f);
                    }

                    else return field;
                }

                else return Vector2.Zero;
            }
            set; } = Vector2.Zero;

        public bool IsEnabled { get; set; } = false;

        public bool IsCentered { get; set; } = true;

        public void NotifyNewPosition(Vector2 postion)
        {
            OffsetPosition = postion;
        }
    }
}
