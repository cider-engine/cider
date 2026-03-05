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
                });
                taskWithDisposable = null;
            }
        }
    }
}
