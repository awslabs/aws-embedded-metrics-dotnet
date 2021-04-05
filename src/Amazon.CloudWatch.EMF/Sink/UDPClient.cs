using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.CloudWatch.EMF.Sink
{
    public class UDPClient : ISocketClient
    {
        private readonly UdpClient _udpClient;

        public UDPClient(Endpoint endpoint)
        {
            _udpClient = new UdpClient(endpoint.Host, endpoint.Port);
        }

        public void Dispose()
        {
            _udpClient.Dispose();
        }

        public async Task SendMessageAsync(string message)
        {
            var data = Encoding.ASCII.GetBytes(message + "\n");
            try
            {
                await _udpClient.SendAsync(data, data.Length);
            }
            catch (Exception)
            {
            }
        }
    }
}