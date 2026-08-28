using Cider.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Components
{
    public class Scene : Component
    {
        /// <summary>
        /// 此场景所属的<c>Window</c>
        /// </summary>
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

        private List<nkast.Aether.Physics2D.Dynamics.Body> BodiesToAdd2D { get; } = new();
        private List<nkast.Aether.Physics2D.Dynamics.Body> BodiesToRemove2D { get; } = new();

        private Lazy<nkast.Aether.Physics2D.Dynamics.World> World2D { get; } = new(() => new()
        {
            Gravity = Game.Instance.ProjectSettings.DefaultGravity.AsPhysicsVector2()
        });

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

            if (BodiesToRemove2D.Count > 0) // 在没有元素的时候调用Clear仍然会修改集合的_version
                BodiesToRemove2D.Clear();
        }

        internal void OnLateUpdate()
        {
            foreach (var item in BodiesToAdd2D)
            {
                World2D.Value.Add(item);
            }

            if (BodiesToAdd2D.Count > 0) // 在没有元素的时候调用Clear仍然会修改集合的_version
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

        internal void EnqueueBodyToAdd2D(ReadOnlySpan<nkast.Aether.Physics2D.Dynamics.Body> bodies)
        {
            BodiesToAdd2D.AddRange(bodies);
        }

        internal void EnqueueBodyToAdd2D(IEnumerable<nkast.Aether.Physics2D.Dynamics.Body> bodies)
        {
            BodiesToAdd2D.AddRange(bodies);
        }

        internal void EnqueueBodyToRemove2D(ReadOnlySpan<nkast.Aether.Physics2D.Dynamics.Body> bodies)
        {
            BodiesToRemove2D.AddRange(bodies);
        }

        internal void EnqueueBodyToRemove2D(IEnumerable<nkast.Aether.Physics2D.Dynamics.Body> bodies)
        {
            BodiesToRemove2D.AddRange(bodies);
        }
    }
}
