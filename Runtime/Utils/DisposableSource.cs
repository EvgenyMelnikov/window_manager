using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace WindowManager
{
    public class DisposableSource : IDisposable
    {
        private readonly Action _disposeAction;

        public DisposableSource(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction();
        }
    }
    
    public class AsyncDisposableSource : IAsyncDisposable
    {
        private readonly Func<UniTask> _disposeAction;

        public AsyncDisposableSource(Func<UniTask> disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public async ValueTask DisposeAsync()
        {
            await _disposeAction.Invoke();
        }
    }
}