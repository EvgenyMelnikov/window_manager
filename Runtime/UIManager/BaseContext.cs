using Cysharp.Threading.Tasks;

namespace WindowManager
{
    internal abstract class BaseContext
    {
        public abstract UniTask ExecuteClose();
        
        internal abstract UniTask Prepare();
        internal abstract UniTask Open();
        internal abstract UniTask Show();
        internal abstract UniTask Hide();
        internal abstract UniTask Close();
        internal abstract void Dispose();
    }
}