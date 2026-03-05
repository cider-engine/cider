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
                Game.Instance.TryRaiseException(task.Exception);
            }

            public Task<T> EnsureToBeSuccessful()
            {
                task.ContinueWith(x => x.EnsureSuccess());
                return task;
            }
        }
    }
}
