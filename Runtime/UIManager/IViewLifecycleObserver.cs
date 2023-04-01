namespace WindowManager
{
    public interface IViewLifecycleObserver
    {
        void OpenStart();
        void OpenComplete();
        void CloseStart();
        void CloseComplete();
    }
}