using System;

namespace Amazon.CloudWatch.EMF.Sink
{
    public class Endpoint
    {
        public static readonly Endpoint DEFAULT_TCP_ENDPOINT = new Endpoint("127.0.0.1", 25888, Protocol.TCP);

        public string Host { get; private set; }

        public int Port { get; private set; }

        public Protocol CurrentProtocol { get; private set; }

        private Endpoint(string host, int port, Protocol protocol)
        {
            Host = host;
            Port = port;
            CurrentProtocol = protocol;
        }

        public static Endpoint FromURL(string endpoint)
        {
            Uri parsedUri = null;

            try
            {
                parsedUri = new Uri(endpoint);
            }
            catch (UriFormatException ex)
            {
                // log.warn("Failed to parse the endpoint: {} ", endpoint);
                return DEFAULT_TCP_ENDPOINT;
            }

            if (parsedUri.Host == null
                || parsedUri.Port < 0
                || parsedUri.Scheme == null)
            {
                return DEFAULT_TCP_ENDPOINT;
            }

            Protocol protocol;
            try
            {
                protocol = GetProtocol(parsedUri.Scheme);
            }

            // Catch IllegalArgumentException
            catch (Exception e)
            {
                /*log.warn(
                    "Unsupported protocol: {}. Would use default endpoint: {}",
                    parsedURI.getScheme(),
                    DEFAULT_TCP_ENDPOINT);*/

                return DEFAULT_TCP_ENDPOINT;
            }

            return new Endpoint(parsedUri.Host, parsedUri.Port, protocol);
        }

        public string ToString()
        {
            return CurrentProtocol.ToString().ToLower() + "://" + Host + ":" + Port;
        }

        // TODO: fix this
        private static Protocol GetProtocol(string value)
        {
            foreach (var protocol in Enum.GetValues(typeof(Protocol)))
            {
                if (protocol.ToString().Equals(value))
                {
                    return Protocol.TCP;
                }
            }

            return Protocol.TCP;
        }
    }
}