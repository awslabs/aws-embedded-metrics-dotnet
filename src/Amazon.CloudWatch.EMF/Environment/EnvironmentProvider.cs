using System;
using Amazon.CloudWatch.EMF.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Amazon.CloudWatch.EMF.Environment
{
    /// <summary>
    /// A provider that will detect the environment.
    /// </summary>
    public class EnvironmentProvider : IEnvironmentProvider
    {
        private readonly IConfiguration _configuration;
        private readonly IResourceFetcher _resourceFetcher;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ECSEnvironment> _logger;
        private IEnvironment _cachedEnvironment;

        public EnvironmentProvider(IConfiguration configuration, IResourceFetcher resourceFetcher)
            : this(configuration, resourceFetcher, NullLoggerFactory.Instance)
        {
        }

        public EnvironmentProvider(IConfiguration configuration, IResourceFetcher resourceFetcher, ILoggerFactory loggerFactory)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _resourceFetcher = resourceFetcher ?? throw new ArgumentNullException(nameof(resourceFetcher));
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ECSEnvironment>();
        }

        /// <summary>
        ///  Find the current environment
        /// </summary>
        /// <returns></returns>
        public IEnvironment ResolveEnvironment()
        {
            if (_cachedEnvironment != null)
                return _cachedEnvironment;

            var env = GetEnvironmentFromConfig();
            if (env != null)
            {
                _logger.LogDebug("Loaded environment from config: {EnvName}", env.GetType());
                _cachedEnvironment = env;
                return _cachedEnvironment;
            }

            env = GetEnvironmentByProbe();
            if (env != null)
            {
                _logger.LogDebug("Detected environment: {EnvName}", env.GetType());
                _cachedEnvironment = env;
                return _cachedEnvironment;
            }

            _logger.LogDebug("Failed to detect environment, using default.");
            return new DefaultEnvironment(_configuration, _loggerFactory);
        }

        private IEnvironment GetEnvironmentFromConfig()
        {
            switch (_configuration.EnvironmentOverride)
            {
                case Environments.Lambda:
                    return new LambdaEnvironment(_configuration, _loggerFactory);
                case Environments.Agent:
                    return new DefaultEnvironment(_configuration, _loggerFactory);
                case Environments.EC2:
                    return new EC2Environment(_configuration, _resourceFetcher, _loggerFactory);
                case Environments.ECS:
                    return new ECSEnvironment(_configuration, _resourceFetcher, _loggerFactory);
                case Environments.Local:
                    return new LocalEnvironment(_configuration, _loggerFactory);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Do not change the order of environment probe conditions.
        /// </summary>
        /// <returns></returns>
        private IEnvironment GetEnvironmentByProbe()
        {
            IEnvironment environment = new LambdaEnvironment(_configuration, _loggerFactory);
            if (environment.Probe()) return environment;

            environment = new ECSEnvironment(_configuration, _resourceFetcher, _loggerFactory);
            if (environment.Probe()) return environment;

            environment = new EC2Environment(_configuration, _resourceFetcher, _loggerFactory);
            if (environment.Probe()) return environment;

            environment = new DefaultEnvironment(_configuration, _loggerFactory);
            return environment.Probe() ? environment : null;
        }
    }
}