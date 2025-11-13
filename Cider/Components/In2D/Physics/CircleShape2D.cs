using MonoGame.Extended.ECS;
using nkast.Aether.Physics2D.Dynamics;
using System;

namespace Cider.Components.In2D.Physics
{
    public class CircleShape2D : Shape2D
    {
        public float Radius { get; set; }

        public float Density { get; set; } = 1.0f;

#nullable enable
        private Fixture? _fixture;

        public override Body? Body => _fixture?.Body;

        public override void Attach(Body body, bool isSensor = false)
        {
            if (_fixture is not null)
                throw new InvalidOperationException("Shape is already attached to a body.");
            _fixture = body.CreateCircle(Radius, Density, Position);
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
