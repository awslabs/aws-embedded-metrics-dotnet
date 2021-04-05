using Amazon.CloudWatch.EMF.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Amazon.CloudWatch.EMF.Environment
{
    public class DefaultEnvironment : AgentBasedEnvironment
    {
        public DefaultEnvironment(IConfiguration configuration)
        : base(configuration, NullLoggerFactory.Instance)
        {
        }

        public DefaultEnvironment(IConfiguration configuration, ILoggerFactory loggerFactory)
        : base(configuration, loggerFactory)
        {
        }
    }
}