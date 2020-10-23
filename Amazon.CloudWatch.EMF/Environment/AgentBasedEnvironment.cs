using System;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;

namespace Amazon.CloudWatch.EMF.Environment
{
    public abstract class AgentBasedEnvironment : IEnvironment
    {
        protected readonly IConfiguration _configuration;
        private ISink _sink;

        protected AgentBasedEnvironment(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public virtual string Name => !string.IsNullOrEmpty(_configuration.ServiceName) ? _configuration.ServiceName : Constants.UNKNOWN;

        public virtual string Type => !string.IsNullOrEmpty(_configuration.ServiceType) ? _configuration.ServiceType : Constants.UNKNOWN;

        public virtual string LogGroupName => !string.IsNullOrEmpty(_configuration.LogGroupName) ? _configuration.LogGroupName : Name + "_metrics";

        public virtual string LogStreamName => !string.IsNullOrEmpty(_configuration.LogStreamName) ? _configuration.LogStreamName : string.Empty;

        public ISink Sink
        {
            get
            {
                if (_sink == null)
                {
                    Endpoint endpoint;
                    endpoint = string.IsNullOrEmpty(_configuration.AgentEndPoint) ? Endpoint.DEFAULT_TCP_ENDPOINT : Endpoint.FromURL(_configuration.AgentEndPoint);

                    _sink = new AgentSink(
                        LogGroupName,
                        LogStreamName,
                        endpoint,
                        new SocketClientFactory());
                }

                return _sink;
            }
        }

        public virtual bool Probe()
        {
            return true;
        }

        public virtual void ConfigureContext(MetricsContext context)
        {
        }
    }
}