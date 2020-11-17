namespace Amazon.CloudWatch.EMF.Sink
{
    public interface ISocketClientFactory
    {
        ISocketClient GetClient(Endpoint endpoint);
    }

    public class SocketClientFactory : ISocketClientFactory
    {
        public ISocketClient GetClient(Endpoint endpoint)
        {
             if (endpoint.CurrentProtocol == Protocol.UDP)
             {
                 return new UDPClient(endpoint);
             }

             return new TCPClient(endpoint);
        }
    }
}