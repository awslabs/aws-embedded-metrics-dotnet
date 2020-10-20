using System;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;

namespace Amazon.CloudWatch.EMF.Environment
{
    public abstract class AgentBasedEnvironment : IEnvironment
    {
        protected IConfiguration _configuration;
        private ISink _sink;

        protected AgentBasedEnvironment(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string Name => !string.IsNullOrEmpty(_configuration.ServiceName) ? _configuration.ServiceName : Constants.UNKNOWN;

        public string Type => !string.IsNullOrEmpty(_configuration.ServiceType) ? _configuration.ServiceType : Constants.UNKNOWN;

        public string LogGroupName => !string.IsNullOrEmpty(_configuration.LogGroupName) ? _configuration.LogGroupName : Name + "_metrics";

        public string LogStreamName => !string.IsNullOrEmpty(_configuration.LogStreamName) ? _configuration.LogStreamName : string.Empty;

        public ISink Sink
        {
            get
            {
                if (_sink == null)
                {
                    Endpoint endpoint;
                    if (string.IsNullOrEmpty(_configuration.AgentEndPoint))
                    {
                        // log.info("Endpoint is not defined. Using default: {}",
                        // Endpoint.DEFAULT_TCP_ENDPOINT);
                        endpoint = Endpoint.DEFAULT_TCP_ENDPOINT;
                    }
                    else
                    {
                        endpoint = Endpoint.FromURL(_configuration.AgentEndPoint);
                    }

                    _sink = new AgentSink(
                        LogGroupName,
                        LogStreamName,
                        endpoint,
                        new SocketClientFactory());
                }

                return _sink;
            }
        }

        public bool Probe()
        {
            return true;
        }

        public void ConfigureContext(MetricsContext context)
        {
        }
    }
}