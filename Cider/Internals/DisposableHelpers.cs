using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cider.Internals
{
    public static class DisposableHelpers
    {
#nullable enable
        public static void DisposeAndSetNull<T>(ref T? disposable) where T : IDisposable
        {
            disposable?.Dispose();
            disposable = default;
        }
        public static void DisposeAndSetNull<T>(ref Task<T>? taskWithDisposable) where T : IDisposable
        {
            if (taskWithDisposable?.IsCompletedSuccessfully == true)
            {
                taskWithDisposable.Result.Dispose();
                taskWithDisposable = null;
            }

            else
            {
                taskWithDisposable?.ContinueWith(static x =>
                {
                    if (x.IsCompletedSuccessfully) x.Result.Dispose();
                }, TaskScheduler.FromCurrentSynchronizationContext());
                taskWithDisposable = null;
            }
        }
        public static void DisposeAndSetNull<T1, T2>(ref Task<(T1, T2)>? taskWithDisposable) where T1 : IDisposable where T2 : IDisposable
        {
            if (taskWithDisposable?.IsCompletedSuccessfully == true)
            {
                var (_1, _2) = taskWithDisposable.Result;
                _1.Dispose();
                _2.Dispose();
                taskWithDisposable = null;
            }

            else
            {
                taskWithDisposable?.ContinueWith(static x =>
                {
                    if (x.IsCompletedSuccessfully)
                    {
                        var (_1, _2) = x.Result;
                        _1.Dispose();
                        _2.Dispose();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
                taskWithDisposable = null;
            }
        }
        public static void DisposeAndSetNull<T>(ref IEnumerable<Task<T>>? tasksWithDisposable) where T : IDisposable
        {
            if (tasksWithDisposable is null) return;

            foreach (var taskWithDisposable in tasksWithDisposable)
            {
                if (taskWithDisposable?.IsCompletedSuccessfully == true)
                {
                    taskWithDisposable.Result.Dispose();
                }

                else
                {
                    taskWithDisposable?.ContinueWith(static x =>
                    {
                        if (x.IsCompletedSuccessfully) x.Result.Dispose();
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }

            tasksWithDisposable = null;
        }
    }
}
