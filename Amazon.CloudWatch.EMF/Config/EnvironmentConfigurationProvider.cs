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
                            getEnvVar(ConfigurationKeys.SERVICE_NAME),
                            getEnvVar(ConfigurationKeys.SERVICE_TYPE),
                            getEnvVar(ConfigurationKeys.LOG_GROUP_NAME),
                            getEnvVar(ConfigurationKeys.LOG_STREAM_NAME),
                            getEnvVar(ConfigurationKeys.AGENT_ENDPOINT),
                            getEnvironmentOverride());
                }
                return _config;
            }
        }
        
        private static string GetEnvVar(String key) 
        {
            string name = string.join("", ConfigurationKeys.ENV_VAR_PREFIX, "_", key);
            return GetEnv(name);
        }
        
        private static string GetEnv(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }
        
        private static Environments EnvironmentOverride
        {
           /* string environmentName = GetEnvVar(ConfigurationKeys.ENVIRONMENT_OVERRIDE);
            if (string.IsNullOrEmpty(environmentName))
            {
                return Environments.Unknown;
            }

            try {
                return Environments.valueOf(environmentName);
            } catch (Exception e) 
            {
                return Environments.Unknown;
            }
        }
    }
