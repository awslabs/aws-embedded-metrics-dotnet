using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Amazon.CloudWatch.EMF.Sink
{
    public class Endpoint
    {
        public static readonly Endpoint DEFAULT_TCP_ENDPOINT = new Endpoint("127.0.0.1", 25888, Protocol.TCP);

        public string Host { get; private set; }

        public int Port { get; private set; }

        public Protocol CurrentProtocol { get; private set; }

        public Endpoint(string url) : this(url, NullLoggerFactory.Instance)
        {
        }

        public Endpoint(string url, ILoggerFactory loggerFactory)
        {
            Uri parsedUri = null;

            loggerFactory ??= NullLoggerFactory.Instance;
            ILogger logger = loggerFactory.CreateLogger<Endpoint>();

            try
            {
                parsedUri = new Uri(url);
            }
            catch (UriFormatException)
            {
                logger.LogWarning("Failed to parse the endpoint: {} ", url);
                SetDefault();
            }

            if (parsedUri == null || parsedUri.Port < 0)
            {
                SetDefault();
            }

            try
            {
                var protocol = GetProtocol(parsedUri.Scheme);

                Host = parsedUri.Host;
                Port = parsedUri.Port;
                CurrentProtocol = protocol;
            }
            catch (Exception)
            {
                logger.LogWarning(
                    "Unsupported protocol: {}. Would use default endpoint: {}",
                    url,
                    DEFAULT_TCP_ENDPOINT);

                SetDefault();
            }
        }

        private Endpoint(string host, int port, Protocol protocol)
        {
            Host = host;
            Port = port;
            CurrentProtocol = protocol;
        }

        public override string ToString()
        {
            return CurrentProtocol.ToString().ToLower() + "://" + Host + ":" + Port;
        }

        private static Protocol GetProtocol(string value)
        {
            foreach (Protocol protocol in Enum.GetValues(typeof(Protocol)))
            {
                if (protocol.ToString().Equals(value, StringComparison.CurrentCultureIgnoreCase))
                {
                    return protocol;
                }
            }

            return Protocol.TCP;
        }

        private void SetDefault()
        {
            var defaultEndpoint = DEFAULT_TCP_ENDPOINT;
            Host = defaultEndpoint.Host;
            Port = defaultEndpoint.Port;
            CurrentProtocol = defaultEndpoint.CurrentProtocol;
        }
    }
}