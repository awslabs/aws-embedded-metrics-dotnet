using System;
using Amazon.CloudWatch.EMF.Environment;
using Amazon.CloudWatch.EMF.Utils;

namespace Amazon.CloudWatch.EMF.Config
{
    /// <summary>
    /// Loads configuration from environment variables.
    /// </summary>
    public class EnvironmentConfigurationProvider
    {
        private static IConfiguration _config;

        public static IConfiguration Config
        {
            get
            {
                var bufferSize = Int32.TryParse(
                        GetEnvVar(ConfigurationKeys.AgentBufferSize), out var parsedBufferSize)
                            ? parsedBufferSize
                            : Configuration.DEFAULT_AGENT_BUFFER_SIZE;

                return _config ??= new Configuration(
                    GetEnvVar(ConfigurationKeys.ServiceName),
                    GetEnvVar(ConfigurationKeys.ServiceType),
                    GetEnvVar(ConfigurationKeys.LogGroupName),
                    GetEnvVar(ConfigurationKeys.LogStreamName),
                    GetEnvVar(ConfigurationKeys.AgentEndpoint),
                    bufferSize,
                    GetEnvironmentOverride());
            }
            set => _config = value;
        }

        private static Environments GetEnvironmentOverride()
        {
            var environmentName = GetEnvVar(ConfigurationKeys.EnvironmentOverride);
            if (string.IsNullOrEmpty(environmentName))
            {
                return Environments.Unknown;
            }

            try
            {
                return (Environments)Enum.Parse(typeof(Environments), environmentName);
            }
            catch (Exception)
            {
                return Environments.Unknown;
            }
        }

        private static string GetEnvVar(string key)
        {
            var name = string.Join(string.Empty, ConfigurationKeys.EnvVarPrefix, "_", key);
            return EnvUtils.GetEnv(name);
        }
    }
}