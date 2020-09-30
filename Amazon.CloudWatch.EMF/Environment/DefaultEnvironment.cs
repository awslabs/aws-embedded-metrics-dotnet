using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;
using Amazon.CloudWatch.EMF.Config;

namespace Amazon.CloudWatch.EMF.Environment
{
    public class DefaultEnvironment : AgentBasedEnvironment
    {
        public new bool Probe()
        {
            return true;
        }

        public new void ConfigureContext(MetricsContext context)
        {
        }
    }
}
