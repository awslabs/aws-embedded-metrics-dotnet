using System.Net;

namespace Amazon.CloudWatch.EMF.Sink
{
    public class SocketClientFactory
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