using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WindowManager
{
    public static class TasksUtils
    {
        private const int FrameTime = 1000 / 60;

        public static async void CancelAfter<T>(this TaskCompletionSource<T> source, TimeSpan delay)
        {
            if (source == null)
                return;

            var cancellation = new CancellationTokenSource();
            cancellation.CancelAfter(delay);

            try
            {
                while (!cancellation.Token.IsCancellationRequested && !source.Task.IsCanceled &&
                       !source.Task.IsCompleted && !source.Task.IsFaulted)
                {
                    await Task.Delay(100, cancellation.Token);
                }

                if (!source.Task.IsCanceled)
                    source.TrySetCanceled();
            }
            catch (Exception)
            {
                /*ignore*/
            }
        }

        public static async Task WaitUntil(Func<bool> predicate, CancellationToken cancellationToken = default,
            int milliseconds = FrameTime)
        {
            try
            {
                while (!predicate.Invoke())
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    await Task.Delay(milliseconds, cancellationToken);
                }
            }
            catch (Exception)
            {
                /*ignore*/
            }
        }

        public static void Forget(this Task task)
        {
            task.ContinueWith(p => Debug.LogException(p.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }

        public static async Task<T> WithFastCancellationSafe<T>(this Task<T> task, CancellationToken token,
            T cancelValue = default)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (token.Register(() => tcs.SetResult(true)))
            {
                await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);
                if (task.IsCompleted)
                {
                    return await task.ConfigureAwait(false);
                }

                task.Forget();
                System.Diagnostics.Debug.Assert(tcs.Task.IsCompleted, "Task supposed to be completed in this case");
                return cancelValue;
            }
        }

        public static async Task SkipCancelException(this Task target)
        {
            try
            {
                await target;
            }
            catch (OperationCanceledException)
            {
            }
        }

        public static Task FromCallback(Action<Action> action, CancellationToken cancellationToken = default)
        {
            var taskSource = new TaskCompletionSource<object>();
            action(() => taskSource.TrySetResult(default));
            return taskSource.Task.WithFastCancellationSafe(cancellationToken);
        }

        public static Task<T> FromCallback<T>(Action<Action<T>> action, CancellationToken cancellationToken = default)
        {
            var taskSource = new TaskCompletionSource<T>();
            action(result => taskSource.TrySetResult(result));
            return taskSource.Task.WithFastCancellationSafe(cancellationToken);
        }

        public static async Task RunInMainThread(this Task source)
        {
            await UniTask.SwitchToMainThread();
            await source;
        }

        public static async Task<T> RunInMainThread<T>(this Task<T> source)
        {
            await UniTask.SwitchToMainThread();
            return await source;
        }
    }
}

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