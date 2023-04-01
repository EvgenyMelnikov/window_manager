namespace WindowManager
{
    public interface IDataReceiver<in TData>
    {
        void SetData(TData data);
    }
}