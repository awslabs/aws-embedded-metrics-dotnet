using System;
using System.Net.Sockets;
using System.Text;

namespace Amazon.CloudWatch.EMF.Sink
{
    public class UDPClient : ISocketClient
    {
        private readonly UdpClient _udpClient;

        public UDPClient(Endpoint endpoint)
        {
            _udpClient = new UdpClient(endpoint.Host, endpoint.Port);
        }

        public void SendMessage(string message)
        {
            var data = Encoding.ASCII.GetBytes(message + "\n");
            try
            {
                _udpClient.Send(data, data.Length);
            }
            catch (Exception)
            {
            }
        }
    }
}