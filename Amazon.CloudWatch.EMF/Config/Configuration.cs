using Amazon.CloudWatch.EMF.Environment;

namespace Amazon.CloudWatch.EMF.Config
{
    public class Configuration
    {
        /// <summary>
        /// Gets name of the service to use in the default dimensions
        /// </summary>
        public string ServiceName { get; private set; }

        /// <summary>
        /// Gets the type of the service to use in the default dimensions.
        /// </summary>
        public string ServiceType { get; private set; }

        /// <summary>
        /// Gets the LogGroup name to use.
        /// This is only used for the Cloudwatch Agent in agent-based environment.
        /// </summary>
        public string LogGroupName { get; private set; }

        /// <summary>
        /// Gets the LogStream name to use. This will be ignored when using the Lambda scope.
        /// </summary>
        public string LogStreamName { get; private set; }

        /// <summary>
        /// Gets the endpoint to use to connect to the CloudWatch Agent.
        /// </summary>
        public string AgentEndPoint { get; private set; }

        /// <summary>
        /// Environment override. This will short circuit auto-environment detection. Valid values
        /// include: - Local: no decoration and sends over stdout - Lambda: decorates logs with Lambda
        /// metadata and sends over stdout - Agent: no decoration and sends over TCP - EC2: decorates
        /// logs with EC2 metadata and sends over TCP
        /// </summary>
        public Environments EnvironmentOverride { get; private set; }
    }
}