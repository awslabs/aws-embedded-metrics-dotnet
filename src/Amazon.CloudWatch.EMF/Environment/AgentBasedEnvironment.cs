using System;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;
using Microsoft.Extensions.Logging;

namespace Amazon.CloudWatch.EMF.Environment
{
public abstract class AgentBasedEnvironment : IEnvironment
    {
        protected readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        private ISink _sink;

        protected AgentBasedEnvironment(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _loggerFactory = loggerFactory;
        }

        public virtual string Name => !string.IsNullOrEmpty(_configuration.ServiceName) ? _configuration.ServiceName : Constants.UNKNOWN;

        public virtual string Type => !string.IsNullOrEmpty(_configuration.ServiceType) ? _configuration.ServiceType : Constants.UNKNOWN;

        public virtual string LogGroupName => !string.IsNullOrEmpty(_configuration.LogGroupName) ? _configuration.LogGroupName : Name + "_metrics";

        public virtual string LogStreamName => !string.IsNullOrEmpty(_configuration.LogStreamName) ? _configuration.LogStreamName : string.Empty;

        public ISink Sink
        {
            get
            {
                if (_sink != null) return _sink;

                var endpoint = string.IsNullOrEmpty(_configuration.AgentEndPoint)
                    ? Endpoint.DEFAULT_TCP_ENDPOINT
                    : new Endpoint(_configuration.AgentEndPoint);

                _sink = new AgentSink(
                    LogGroupName,
                    LogStreamName,
                    endpoint,
                    new SocketClientFactory(),
                    _configuration,
                    _loggerFactory);

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