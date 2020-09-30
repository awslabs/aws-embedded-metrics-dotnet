namespace Amazon.CloudWatch.EMF.Sink
{
    public interface SocketClient
    {
        void SendMessage(string message);
    }
}