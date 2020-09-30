using System;
using System.Dynamic;

namespace Amazon.CloudWatch.EMF.Config
{
    using System;
    using Amazon.CloudWatch.EMF.Environment;

    public class EnvironmentConfigurationProvider
    {
        private static Configuration _config;

        public static Configuration Config
        {
            get
            {
                if (_config == null)
                {
                    _config =
                        new Configuration(
                            GetEnvVar(ConfigurationKeys.SERVICE_NAME),
                            GetEnvVar(ConfigurationKeys.SERVICE_TYPE),
                            GetEnvVar(ConfigurationKeys.LOG_GROUP_NAME),
                            GetEnvVar(ConfigurationKeys.LOG_STREAM_NAME),
                            GetEnvVar(ConfigurationKeys.AGENT_ENDPOINT),
                            GetEnvironmentOverride());
                }
                return _config;
            }
        }

        private static string GetEnvVar(String key)
        {
            //string name = string.join("", ConfigurationKeys.ENV_VAR_PREFIX, "_", key);
            
            //return GetEnv(name);
            return string.Empty;
        }

        private static string GetEnv(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        private static Environments GetEnvironmentOverride()
        {
            return new Environments();
        }
    }
}
