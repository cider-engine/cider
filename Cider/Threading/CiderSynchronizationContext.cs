#nullable enable
using SDL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TaskType = (System.Threading.SendOrPostCallback d, object? state);

namespace Cider.Threading
{
    public class CiderSynchronizationContext : SynchronizationContext
    {
        internal readonly ConcurrentBag<TaskType> Tasks;

        public static readonly uint EventType;

        static CiderSynchronizationContext()
        {
            EventType = SDL3.SDL_RegisterEvents(1);
        }

        private static unsafe void NotifyNewEvent()
        {
            SDL_Event e = new()
            {
                type = EventType
            };
            SDL3.SDL_PushEvent(&e);
        }

        public CiderSynchronizationContext()
        {
            Tasks = new();
        }

        public CiderSynchronizationContext(IEnumerable<TaskType> tasksToCopy)
        {
            Tasks = new(tasksToCopy);
        }

        public override SynchronizationContext CreateCopy() => new CiderSynchronizationContext(Tasks);

        public override void Post(SendOrPostCallback d, object? state)
        {
            Tasks.Add((d, state));
            NotifyNewEvent();
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            throw new NotSupportedException(); // 禁用同步
        }
    }
}
