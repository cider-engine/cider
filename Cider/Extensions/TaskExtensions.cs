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
                Game.Instance.TryRaiseException(task.Exception ?? new CiderGameException("The task is not completed successfully.") as Exception);
            }

            public void EnsureSuccessOrCancel()
            {
                if (task.IsCompletedSuccessfully || task.IsCanceled) return;
                Game.Instance.TryRaiseException(task.Exception ?? new CiderGameException("The task is not completed successfully or canceled.") as Exception);
            }

            public Task<T> EnsureToBeSuccessful()
            {
                task.ContinueWith(x => x.EnsureSuccess(), Game.GetTaskScheduler());
                return task;
            }

            public Task<T> EnsureToBeSuccessfulOrCanceled()
            {
                task.ContinueWith(x => x.EnsureSuccessOrCancel(), Game.GetTaskScheduler());
                return task;
            }
        }

        extension(Task task)
        {
            public void EnsureSuccess()
            {
                if (task.IsCompletedSuccessfully) return;
                Game.Instance.TryRaiseException(task.Exception ?? new CiderGameException("The task is not completed successfully.") as Exception);
            }

            public void EnsureSuccessOrCancel()
            {
                if (task.IsCompletedSuccessfully || task.IsCanceled) return;
                Game.Instance.TryRaiseException(task.Exception ?? new CiderGameException("The task is not completed successfully or canceled.") as Exception);
            }

            public Task EnsureToBeSuccessful()
            {
                task.ContinueWith(x => x.EnsureSuccess(), Game.GetTaskScheduler());
                return task;
            }

            public Task EnsureToBeSuccessfulOrCanceled()
            {
                task.ContinueWith(x => x.EnsureSuccessOrCancel(), Game.GetTaskScheduler());
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
