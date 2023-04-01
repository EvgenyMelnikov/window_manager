using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WindowManager
{
    public abstract class OverlayViewManager : BaseManager
    {
        internal readonly List<BaseContext> Views = new();

        private protected override BaseContext TopView => Views.Count > 0 ? Views[0] : null;

        private protected override async UniTask Open(BaseContext ctx)
        {
            Views.Add(ctx);
            await ctx.Open();
            await ctx.Show();
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

        private protected override UniTask CloseAllViews()
        {
            return UniTask.WhenAll(Views.Select(async ctx =>
            {
                await ctx.Hide();
                await ctx.Close();
            }));
        }
    }
}