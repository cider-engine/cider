using Cider.Extensions;
using nkast.Aether.Physics2D.Dynamics;
using System;

namespace Cider.Physics
{
    public class CircleShape2D : Shape2D
    {
        public float Radius { get; set; }

        public float Density { get; set; } = 1.0f;

        private Fixture? _fixture;

        public override void Attach(Body body, bool isSensor = false)
        {
            if (_fixture is not null)
                throw new InvalidOperationException("Shape is already attached to a body.");
            _fixture = body.CreateCircle(Radius, Density, Position.AsPhysicsVector2());
            _fixture.IsSensor = isSensor;
            _fixture.Friction = Friction;
        }

        public override void Detach()
        {
            if (_fixture?.Body is { } body)
                body.Remove(_fixture);
            _fixture = null;
        }
    }
}
