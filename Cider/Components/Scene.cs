using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Components
{
    public class Scene : Component
    {
        internal List<nkast.Aether.Physics2D.Dynamics.Body> BodiesToAdd2D { get; } = new();
        internal List<nkast.Aether.Physics2D.Dynamics.Body> BodiesToRemove2D { get; } = new();

        internal nkast.Aether.Physics2D.Dynamics.World World2D { get; } = new()
        {
            Gravity = new(0, 0)
        };

        internal void EnqueueBodyToAdd2D(nkast.Aether.Physics2D.Dynamics.Body body)
        {
            BodiesToAdd2D.Add(body);
        }

        internal void EnqueueBodyToRemove2D(nkast.Aether.Physics2D.Dynamics.Body body)
        {
            BodiesToRemove2D.Add(body);
        }
    }
}
