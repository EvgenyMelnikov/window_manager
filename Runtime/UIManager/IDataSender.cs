namespace WindowManager
{
    public interface IDataSender<out TResult>
    {
        TResult Result { get; }
    }
}