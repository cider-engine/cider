using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cider.Extensions
{
    internal static class TaskExtensions
    {
        extension<T>(Task<T> task)
        {
            public void EnsureSuccess()
            {
                if (task.IsCompletedSuccessfully) return;
                Game.Instance.TryRaiseException(task.Exception ?? new Exception("The task is not completed successfully."));
            }

            public void EnsureSuccessOrCancel()
            {
                if (task.IsCompletedSuccessfully || task.IsCanceled) return;
                Game.Instance.TryRaiseException(task.Exception ?? new Exception("The task is not completed successfully or canceled."));
            }

            public Task<T> EnsureToBeSuccessful()
            {
                task.ContinueWith(x => x.EnsureSuccess(), TaskScheduler.FromCurrentSynchronizationContext());
                return task;
            }

            public Task<T> EnsureToBeSuccessfulOrCanceled()
            {
                task.ContinueWith(x => x.EnsureSuccessOrCancel(), TaskScheduler.FromCurrentSynchronizationContext());
                return task;
            }
        }

        extension(Task task)
        {
            public void EnsureSuccess()
            {
                if (task.IsCompletedSuccessfully) return;
                Game.Instance.TryRaiseException(task.Exception ?? new Exception("The task is not completed successfully."));
            }

            public void EnsureSuccessOrCancel()
            {
                if (task.IsCompletedSuccessfully || task.IsCanceled) return;
                Game.Instance.TryRaiseException(task.Exception ?? new Exception("The task is not completed successfully or canceled."));
            }

            public Task EnsureToBeSuccessful()
            {
                task.ContinueWith(x => x.EnsureSuccess(), TaskScheduler.FromCurrentSynchronizationContext());
                return task;
            }

            public Task EnsureToBeSuccessfulOrCanceled()
            {
                task.ContinueWith(x => x.EnsureSuccessOrCancel(), TaskScheduler.FromCurrentSynchronizationContext());
                return task;
            }
        }

        extension<T>(IEnumerable<Task<T>> tasks)
        {
            public bool IsAllSuccess
            {
                get
                {
                    foreach (var task in tasks)
                        if (!task.IsCompletedSuccessfully) return false;

                    return true;
                }
            }
        }
    }
}
