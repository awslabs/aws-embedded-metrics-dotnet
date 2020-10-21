using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Model;

namespace Amazon.CloudWatch.EMF.Environment
{
    public class DefaultEnvironment : AgentBasedEnvironment
    {
        internal DefaultEnvironment(IConfiguration configuration) : base(configuration)
        {
        }

        public new bool Probe()
        {
            return true;
        }

        public new void ConfigureContext(MetricsContext context)
        {
        }
    }
}