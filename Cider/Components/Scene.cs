using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Components
{
    public class Scene : Component
    {
#nullable enable
        public Window? Window
        {
            get;
            set
            {
                var old = field;
                field = value;
                OnWindowChangedDispatcher(old, value);
            }
        }
#nullable disable
        private List<nkast.Aether.Physics2D.Dynamics.Body> BodiesToAdd2D { get; } = new();
        private List<nkast.Aether.Physics2D.Dynamics.Body> BodiesToRemove2D { get; } = new();

        private Lazy<nkast.Aether.Physics2D.Dynamics.World> World2D { get; } = new(() => new()
        {
            Gravity = new(0, 0)
        });

        public event Action<Scene> FrameEndedOnce;

        internal void OnPhysicsStep(float delta)
        {
            if (World2D.IsValueCreated) World2D.Value.Step(delta);
        }

        internal void OnEarlyUpdate()
        {
            foreach (var item in BodiesToRemove2D)
            {
                World2D.Value.Remove(item);
            }

            BodiesToRemove2D.Clear();
        }

        internal void OnLateUpdate()
        {
            foreach (var item in BodiesToAdd2D)
            {
                World2D.Value.Add(item);
            }

            BodiesToAdd2D.Clear();
        }

        internal void EnqueueBodyToAdd2D(nkast.Aether.Physics2D.Dynamics.Body body)
        {
            BodiesToAdd2D.Add(body);
        }

        internal void EnqueueBodyToRemove2D(nkast.Aether.Physics2D.Dynamics.Body body)
        {
            BodiesToRemove2D.Add(body);
        }

        internal void InvokeFrameEndedOnce()
        {
            FrameEndedOnce?.Invoke(this);
            FrameEndedOnce = null;
        }
    }
}
