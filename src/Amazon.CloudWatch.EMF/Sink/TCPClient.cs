using System.Net.Sockets;
using System.Threading.Tasks;

namespace Amazon.CloudWatch.EMF.Sink
{
    public class TCPClient : ISocketClient
    {
        private readonly Endpoint _endpoint;
        private TcpClient _tcpClient;

        internal TCPClient(Endpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public void Dispose()
        {
            _tcpClient.Close();
        }

        public async Task SendMessageAsync(string message)
        {
            if (_tcpClient == null)
            {
                _tcpClient = new TcpClient(_endpoint.Host, _endpoint.Port);
            }

            // Translate the passed message into ASCII and store it as a Byte array.
            var data = System.Text.Encoding.ASCII.GetBytes(message);

            if (!_tcpClient.Connected)
                await _tcpClient.ConnectAsync(_endpoint.Host, _endpoint.Port);

            // Get a client stream for reading and writing.
            var stream = _tcpClient.GetStream();

            // Send the message to the connected TcpServer.
            await stream.WriteAsync(data, 0, data.Length);
        }
    }
}