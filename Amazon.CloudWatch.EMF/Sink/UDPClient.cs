using System;
using System.Net.Sockets;
using System.Text;

namespace Amazon.CloudWatch.EMF.Sink
{
    public class UDPClient: ISocketClient
    {
        private UdpClient _udpClient;

        public UDPClient(Endpoint endpoint)
        {
            _udpClient = new UdpClient(endpoint.Host, endpoint.Port);
        }
        public void SendMessage(string message)
        {
            Byte[] data = Encoding.ASCII.GetBytes(message);
            try
            {
                _udpClient.Send(data, data.Length);
            }
            catch (Exception e )
            {
            }
        }
    }
}