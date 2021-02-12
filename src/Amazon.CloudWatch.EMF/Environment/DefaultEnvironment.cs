using Amazon.CloudWatch.EMF.Config;
using Microsoft.Extensions.Logging;

namespace Amazon.CloudWatch.EMF.Environment
{
    public class DefaultEnvironment : AgentBasedEnvironment
    {
        public DefaultEnvironment(IConfiguration configuration, ILoggerFactory loggerFactory)
        : base(configuration, loggerFactory)
        {
        }
    }
}