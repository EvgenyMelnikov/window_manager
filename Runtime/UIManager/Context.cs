using Cysharp.Threading.Tasks;

// ReSharper disable SuspiciousTypeConversion.Global

namespace WindowManager
{
    public interface IContext
    {
        UniTask ExecuteClose();
        UniTask WaitForOpenStart();
        UniTask WaitForOpenComplete();
        UniTask WaitForCloseStart();
        UniTask WaitForCloseComplete();
    }

    internal class Context<T> : BaseContext, IContext where T : View
    {
        private UniTaskCompletionSource _openStartSource;
        private UniTaskCompletionSource _openCompleteSource;
        private UniTaskCompletionSource _closeStartSource;
        private UniTaskCompletionSource _closeCompleteSource;
        
        protected readonly BaseManager Manager;
        
        internal View View { get; private set; }

        protected internal Context(BaseManager manager)
        {
            Manager = manager;
        }

        public async UniTask WaitForOpenStart()
        {
            _openStartSource ??= new UniTaskCompletionSource();
            await _openStartSource.Task;
        }

        public async UniTask WaitForOpenComplete()
        {
            _openCompleteSource ??= new UniTaskCompletionSource();
            await _openCompleteSource.Task;
        }

        public async UniTask WaitForCloseStart()
        {
            _closeStartSource ??= new UniTaskCompletionSource();
            await _closeStartSource.Task;
        }

        public async UniTask WaitForCloseComplete()
        {
            _closeCompleteSource ??= new UniTaskCompletionSource();
            await _closeCompleteSource.Task;
        }

        internal override async UniTask Prepare()
        {
            View = await Manager.GetView<T>();
            View.Context = this;
        }

        public override async UniTask ExecuteClose()
        {
            var result = await View.CloseRequest();
            if (result)
                await Manager.Close(this);
        }

        internal override async UniTask Open()
        {
            _openStartSource?.TrySetResult();
            _openStartSource = null;

            await View.OpenStart();
        }

        internal override async UniTask Show()
        {
            await View.ShowStart();
            await View.OpenProcess();
            await View.ShowComplete();
        }

        internal override async UniTask Hide()
        {
            await View.HideStart();
            await View.CloseProcess();
            await View.HideComplete();
        }

        internal override async UniTask Close()
        {
            await View.CloseComplete();

            _closeCompleteSource?.TrySetResult();
            _closeCompleteSource = null;
            
            Dispose();
        }

        internal override void Dispose()
        {
            _openStartSource?.TrySetCanceled();
            _openCompleteSource?.TrySetCanceled();
            _closeStartSource?.TrySetCanceled();
            _closeCompleteSource?.TrySetCanceled();
            
            View.Context = null;
            View = null;
            
            Manager.ReleaseView(View);
        }

        public override string ToString()
        {
            return typeof(T).Name;
        }
    }

    internal class Context<TView, TData> : Context<TView> where TView : View, IDataReceiver<TData>
    {
        private readonly TData _data;

        protected internal Context(TData data, BaseManager manager) : base(manager)
        {
            _data = data;
        }

        internal override UniTask Open()
        {
            (View as IDataReceiver<TData>)?.SetData(_data);

            return base.Open();
        }
    }
}