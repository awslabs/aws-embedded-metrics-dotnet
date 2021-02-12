using Amazon.CloudWatch.EMF.Config;
using Microsoft.Extensions.Logging;

namespace Amazon.CloudWatch.EMF.Environment
{
    public class DefaultEnvironment : AgentBasedEnvironment
    {
        internal DefaultEnvironment(IConfiguration configuration, ILoggerFactory loggerFactory)
        : base(configuration, loggerFactory)
        {
        }
    }
}