using System;
using System.Threading;
using System.Threading.Tasks;

namespace WindowManager
{
    public static class SemaphoreExtensions
    {
        public static async Task<IDisposable> WaitAsyncModal(this SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            return new DisposableSource(() => semaphore.Release());
        }
    }
}