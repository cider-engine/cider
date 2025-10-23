using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Data.In2D.Physics
{
    public class RectangleShape2D : Shape2D
    {
        public float Width { get; set; }

        public float Height { get; set; }

        public float Density { get; set; } = 1.0f;

#nullable enable
        private Fixture? _fixture;

        public override Body? Body => _fixture?.Body;

        public override void Attach(Body body)
        {
            if (_fixture is not null)
                throw new InvalidOperationException("Shape is already attached to a body.");
            _fixture = body.CreateRectangle(Width, Height, Density, new());
        }

        public override void Detach(Body body)
        {
            body.Remove(_fixture);
            _fixture = null;
        }
    }
}
