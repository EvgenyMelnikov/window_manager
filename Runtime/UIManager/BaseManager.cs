using Cysharp.Threading.Tasks;

// ReSharper disable MemberCanBeProtected.Global

namespace WindowManager
{
    public abstract class BaseManager
    {
        protected internal readonly TransitionProcess Transition;
        
        private protected abstract BaseContext TopView { get; }

        protected BaseManager()
        {
            Transition = new TransitionProcess(OnTransitionInited, OnTransitionStarted, OnTransitionCompleted, 1);
        }

        public IContext OpenView<TView>() where TView : View
        {
            return RunOpenProcess(new Context<TView>(this));
        }

        public IContext OpenView<TView, TData>(TData data) where TView : View, IDataReceiver<TData>
        {
            return RunOpenProcess(new Context<TView, TData>(data, this));
        }

        public IContext<TResult> OpenViewWithResult<TView, TResult>() where TView : View, IDataSender<TResult>
        {
            return RunOpenProcess(new ContextWithResult<TView, TResult>(this));
        }

        public IContext<TResult> OpenViewWithResult<TView, TData, TResult>(TData data)
            where TView : View, IDataReceiver<TData>, IDataSender<TResult>
        {
            return RunOpenProcess(new ContextWithResult<TView, TData, TResult>(data, this));
        }

        private TCtx RunOpenProcess<TCtx>(TCtx ctx) where TCtx : BaseContext
        {
            OpenProcess(ctx).Forget();
            return ctx;
        }

        private async UniTask OpenProcess(BaseContext ctx)
        {
            await ctx.Prepare();

            using (await Transition.WaitForFree())
                await Open(ctx);
        }
        
        internal async UniTask Close(BaseContext ctx)
        {
            using (await Transition.WaitForFree())
                await CloseView(ctx);
        }
        
        public IContext CloseLast()
        {
            var ctx = TopView;
            if (ctx == null)
                return null;

            Close(ctx).Forget();
            return ctx as IContext;
        }
        
        public async UniTask CloseAll()
        {
            using (await Transition.WaitForFree())
                await CloseAllViews();
        }
        
        private protected abstract UniTask Open(BaseContext context);
        private protected abstract UniTask CloseAllViews();
        private protected abstract UniTask CloseView(BaseContext ctx);
        
        protected internal abstract UniTask<TView> GetView<TView>()where TView : View;
        protected internal abstract void ReleaseView(View view);
        protected internal virtual UniTask OnTransitionInited() => UniTask.CompletedTask;
        protected internal virtual UniTask OnTransitionStarted() => UniTask.CompletedTask;
        protected internal virtual UniTask OnTransitionCompleted() => UniTask.CompletedTask;
    }
}