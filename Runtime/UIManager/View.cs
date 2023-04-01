using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WindowManager
{
    public class View : MonoBehaviour, IEnumerable
    {
        internal BaseContext Context;
        
        internal void Create() => OnCreate();
        internal UniTask OpenStart() => OnOpenStart();
        internal UniTask ShowStart() => OnShowStart();
        internal UniTask ShowComplete() => OnShowComplete();
        internal UniTask OpenProcess() => PlayOpenAnimation();
        internal UniTask<bool> CloseRequest() => OnCloseRequested();
        internal UniTask HideStart() => OnHideStart();
        internal UniTask HideComplete() => OnHideComplete();
        internal UniTask CloseComplete() => OnCloseComplete();
        internal UniTask CloseProcess() => PlayCloseAnimation();
        internal void Remove() => OnRemove();

        internal void SetActive(bool active) => gameObject.SetActive(active);

        protected void Close()
        {
            Context.ExecuteClose().Forget();
        }

        protected virtual void OnCreate() { }

        protected virtual UniTask OnOpenStart() => UniTask.CompletedTask;
        
        protected virtual UniTask OnShowStart() => UniTask.CompletedTask;
        
        protected virtual UniTask OnShowComplete() => UniTask.CompletedTask;

        protected virtual UniTask OnOpenComplete() => UniTask.CompletedTask;

        protected virtual UniTask PlayOpenAnimation() => UniTask.CompletedTask;

        protected virtual UniTask<bool> OnCloseRequested() => UniTask.FromResult(true);

        protected virtual UniTask OnCloseStart() => UniTask.CompletedTask;
        
        protected virtual UniTask OnHideStart() => UniTask.CompletedTask;
        
        protected virtual UniTask OnHideComplete() => UniTask.CompletedTask;

        protected virtual UniTask OnCloseComplete() => UniTask.CompletedTask;

        protected virtual UniTask PlayCloseAnimation() => UniTask.CompletedTask;
        protected virtual void OnRemove() {}
        public IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}
