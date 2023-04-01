using Cysharp.Threading.Tasks;
// ReSharper disable SuspiciousTypeConversion.Global

namespace WindowManager
{
    public interface IContext<TResult> : IContext
    {
        UniTask<TResult> WaitForResult();
    }
    
    internal class ContextWithResult<TView, TResult> : Context<TView>, IContext<TResult> 
        where TView : View, IDataSender<TResult>
    {
        private readonly IDataSender<TResult> _resultSender;
        
        private UniTaskCompletionSource<TResult> _resultSource;

        protected internal ContextWithResult(BaseManager manager) : base(manager) { }
        
        internal override async UniTask Close()
        {
            await base.Close();
            _resultSource?.TrySetResult((View as IDataSender<TResult>)!.Result);
        }
        
        public UniTask<TResult> WaitForResult()
        {
            _resultSource ??= new UniTaskCompletionSource<TResult>();
            return _resultSource.Task;
        }
    }
    
    internal class ContextWithResult<TView, TData, TResult> : Context<TView, TData>, IContext<TResult> 
        where TView : View, IDataSender<TResult>, IDataReceiver<TData>
    {
        private UniTaskCompletionSource<TResult> _resultSource;
        
        protected internal ContextWithResult(TData data, BaseManager manager) : base(data, manager) { }

        internal override async UniTask Close()
        {
            await base.Close();
            _resultSource?.TrySetResult((View as IDataSender<TResult>)!.Result);
        }
        
        public UniTask<TResult> WaitForResult()
        {
            _resultSource ??= new UniTaskCompletionSource<TResult>();
            return _resultSource.Task;
        }
    }
}