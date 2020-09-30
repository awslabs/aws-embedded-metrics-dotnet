namespace Amazon.CloudWatch.EMF.Config
{
    using System;
    using Amazon.CloudWatch.EMF.Environment;

    /// <summary>
    /// Loads configuration from environment variables.
    /// </summary>
    public class EnvironmentConfigurationProvider
    {
        private static Configuration _config;

        public static Configuration Config
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
        }

        private static string GetEnvVar(string key)
        {
            string name = string.Join("", ConfigurationKeys.ENV_VAR_PREFIX, "_", key);
            return GetEnv(name);
        }

        private static string GetEnv(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        private static Environments GetEnvironmentOverride()
        {
            string environmentName = GetEnvVar(ConfigurationKeys.ENVIRONMENT_OVERRIDE);
            if (string.IsNullOrEmpty(environmentName)) 
            {
                return Environments.Unknown;
            }

            try
            {
                //Get the enum for environmentName
                return (Environments)Enum.Parse(typeof(Environments), environmentName);
            } 
            catch (Exception e) 
            {
                return Environments.Unknown;
            }
        }
    }
}
