namespace Amazon.CloudWatch.EMF.Sink
{
    public interface ISocketClient
    {
        void SendMessage(string message);
    }
}