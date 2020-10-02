using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;

namespace Amazon.CloudWatch.EMF.Environment
{
    public abstract class AgentBasedEnvironment : IEnvironment
    {
        private ISink _sink;
        private Configuration _config;

        protected AgentBasedEnvironment()
        {
        }

        protected AgentBasedEnvironment(Configuration config)
        {
            _config = config;
        }

        public string Name => !string.IsNullOrEmpty(_config.ServiceName) ? _config.ServiceName : Constants.UNKNOWN;

        public string Type => !string.IsNullOrEmpty(_config.ServiceType) ? _config.ServiceType : Constants.UNKNOWN;

        public string LogGroupName => !string.IsNullOrEmpty(_config.LogGroupName) ? _config.LogGroupName : Name + "_metrics";

        public string LogStreamName => !string.IsNullOrEmpty(_config.LogStreamName) ? _config.LogGroupName : "";

        public ISink Sink
        {
            get
            {
                //TODO: Implement Agent Sink and update this
                return _sink;
            }
        }

        public bool Probe()
        {
            throw new System.NotImplementedException();
        }

        public void ConfigureContext(MetricsContext context)
        {
            throw new System.NotImplementedException();
        }

    }
}