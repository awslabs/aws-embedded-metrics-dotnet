using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;
using Microsoft.Extensions.Logging;

namespace Amazon.CloudWatch.EMF.Environment
{
    public class LocalEnvironment : IEnvironment
    {
        private readonly IConfiguration _config;
        private readonly ILoggerFactory _loggerFactory;
        private ISink _sink = null;

        public LocalEnvironment(IConfiguration config, ILoggerFactory loggerFactory)
        {
            _config = config;
            _loggerFactory = loggerFactory;
        }

        public bool Probe()
        {
            return false;
        }

        public string Name => !string.IsNullOrEmpty(_config.ServiceName) ? _config.ServiceName : Constants.UNKNOWN;

        public string Type => !string.IsNullOrEmpty(_config.ServiceType) ? _config.ServiceType : Constants.UNKNOWN;

        public string LogGroupName => !string.IsNullOrEmpty(_config.LogGroupName) ? _config.LogGroupName : Name + "_metrics";

        public void ConfigureContext(MetricsContext context)
        {
        }

        public ISink Sink
        {
            get
            {
                return _sink ??= new ConsoleSink(_loggerFactory);
            }
        }
    }
}
