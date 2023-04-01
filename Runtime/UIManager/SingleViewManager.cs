using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WindowManager
{
    public abstract class SingleViewManager : BaseManager
    {
        internal readonly List<BaseContext> Views = new();

        private protected override BaseContext TopView => Views.Count > 0 ? Views[0] : null;

        private protected override async UniTask Open(BaseContext context)
        {
            if (TopView != null)
                await TopView.Hide();

            Views.Insert(0, context);
            await context.Open();
            await context.Show();
        }
        
        private protected override async UniTask CloseView(BaseContext ctx)
        {
            if (!Views.Remove(ctx))
            {
                Debug.LogError($"Close process view {ctx} out of context");
                return;
            }
            
            await ctx.Hide();
            await ctx.Close();
            ctx.Dispose();
        }

        private protected override async UniTask CloseAllViews()
        {
            await UniTask.WhenAll(CloseTop(), CloseAnother());
            Views.Clear();

            async UniTask CloseTop()
            {
                if (Views.Count > 0)
                {
                    var ctx = Views[0];
                    await ctx.Hide();
                    await ctx.Close();
                    ctx.Dispose();
                }
            }

            async UniTask CloseAnother()
            {
                if (Views.Count > 1)
                {
                    var range = Views.GetRange(1, Views.Count - 2);
                    await UniTask.WhenAll(range.Select(async ctx =>
                    {
                        await ctx.Close();
                        ctx.Dispose();
                    }));
                }
            }
        }
    }
}