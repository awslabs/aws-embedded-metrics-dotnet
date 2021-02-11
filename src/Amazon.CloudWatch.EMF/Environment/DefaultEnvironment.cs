using Amazon.CloudWatch.EMF.Config;

namespace Amazon.CloudWatch.EMF.Environment
{
    public class DefaultEnvironment : AgentBasedEnvironment
    {
        internal DefaultEnvironment(IConfiguration configuration) : base(configuration)
        {
        }
    }
}