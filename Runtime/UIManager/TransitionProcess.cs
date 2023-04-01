using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace WindowManager
{
    public class TransitionProcess : SemaphoreSlim
    {
        private readonly Func<UniTask> _onInit;
        private readonly Func<UniTask> _onStart;
        private readonly Func<UniTask> _onRelease;

        public TransitionProcess(Func<UniTask> onInit, Func<UniTask> onStart, Func<UniTask> onRelease, int initialCount) : base(initialCount)
        {
            _onInit = onInit;
            _onStart = onStart;
            _onRelease = onRelease;
        }
            
        public async UniTask<IDisposable> WaitForFree()
        {
            await _onInit.Invoke();
            await WaitAsync();
            await _onStart.Invoke();
            return new DisposableSource(DisposeAction);
        }

        private async void DisposeAction()
        {
            Release();
            await _onRelease.Invoke();
        }
    }
}