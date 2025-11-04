using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Components.In2D.Physics
{
    public class RectangleShape2D : Shape2D
    {
        public float Width { get; set; }

        public float Height { get; set; }

        public float Density { get; set; } = 1.0f;

#nullable enable
        private Fixture? _fixture;

        public override Body? Body => _fixture?.Body;

        public override void Attach(Body body, bool isSensor = false)
        {
            if (_fixture is not null)
                throw new InvalidOperationException("Shape is already attached to a body.");
            _fixture = body.CreateRectangle(Width, Height, Density, Position);
            _fixture.IsSensor = isSensor;
        }

        public override void Detach(Body body)
        {
            if (_fixture?.Body is not null)
                body.Remove(_fixture);
            _fixture = null;
        }
    }
}
