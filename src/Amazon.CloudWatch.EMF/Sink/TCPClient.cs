using System.Net.Sockets;

namespace Amazon.CloudWatch.EMF.Sink
{
    public class TCPClient : ISocketClient
    {
        private readonly Endpoint _endpoint;
        private readonly TcpClient _tcpClient;

        internal TCPClient(Endpoint endpoint)
        {
            _endpoint = endpoint;
            _tcpClient = new TcpClient(endpoint.Host, endpoint.Port);
        }

        public void SendMessage(string message)
        {
            // Translate the passed message into ASCII and store it as a Byte array.
            var data = System.Text.Encoding.ASCII.GetBytes(message);

            if (!_tcpClient.Connected)
                _tcpClient.Connect(_endpoint.Host, _endpoint.Port);

            // Get a client stream for reading and writing.
            var stream = _tcpClient.GetStream();

            // Send the message to the connected TcpServer.
            stream.Write(data, 0, data.Length);

            // Close everything.
            stream.Close();
            _tcpClient.Close();
        }
    }
}