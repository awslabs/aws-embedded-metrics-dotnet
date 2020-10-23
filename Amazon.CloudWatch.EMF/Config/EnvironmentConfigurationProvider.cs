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
                return _config ??= new Configuration(
                    GetEnvVar(ConfigurationKeys.SERVICE_NAME),
                    GetEnvVar(ConfigurationKeys.SERVICE_TYPE),
                    GetEnvVar(ConfigurationKeys.LOG_GROUP_NAME),
                    GetEnvVar(ConfigurationKeys.LOG_STREAM_NAME),
                    GetEnvVar(ConfigurationKeys.AGENT_ENDPOINT),
                    GetEnvironmentOverride());
            }
            set => _config = value;
        }

        private static Environments GetEnvironmentOverride()
        {
            var environmentName = GetEnvVar(ConfigurationKeys.ENVIRONMENT_OVERRIDE);
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
            var name = string.Join(string.Empty, ConfigurationKeys.ENV_VAR_PREFIX, "_", key);
            return EnvUtils.GetEnv(name);
        }
    }
}