using System;
using Amazon.CloudWatch.EMF.Config;

namespace Amazon.CloudWatch.EMF.Environment
{
    /// <summary>
    /// A provider that will detect the environment.
    /// </summary>
    public class EnvironmentProvider : IEnvironmentProvider
    {
        private readonly IConfiguration _configuration;
        private readonly IResourceFetcher _resourceFetcher;
        private IEnvironment _cachedEnvironment;

        internal IEnvironment DefaultEnvironment => new DefaultEnvironment(_configuration);

        public EnvironmentProvider(IConfiguration configuration, IResourceFetcher resourceFetcher)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _resourceFetcher = resourceFetcher ?? throw new ArgumentNullException(nameof(resourceFetcher));
        }

        /// <summary>
        ///  Find the current environment
        /// </summary>
        /// <returns></returns>
        public Task<IEnvironment> ResolveEnvironment()
        {
            if (_cachedEnvironment != null)
                return _cachedEnvironment;

            var env = GetEnvironmentFromConfig();
            if (env != null)
            {
                _cachedEnvironment = env;
                return _cachedEnvironment;
            }

            env = GetEnvironmentByProbe();
            if (env != null)
            {
                _cachedEnvironment = env;
                return _cachedEnvironment;
            }

            return DefaultEnvironment;
        }

        private IEnvironment GetEnvironmentFromConfig()
        {
            switch (_configuration.EnvironmentOverride)
            {
                case Environments.Lambda:
                    return new LambdaEnvironment();
                case Environments.Agent:
                    return new DefaultEnvironment(_configuration);
                case Environments.EC2:
                    return new EC2Environment(_configuration, _resourceFetcher);
                case Environments.ECS:
                    return new ECSEnvironment(_configuration, _resourceFetcher);
                case Environments.Local:
                    return new LocalEnvironment(_configuration);
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
            IEnvironment environment = new LambdaEnvironment();
            if (environment.Probe()) return environment;

            environment = new ECSEnvironment(_configuration, _resourceFetcher);
            if (environment.Probe()) return environment;

            environment = new EC2Environment(_configuration, _resourceFetcher);
            if (environment.Probe()) return environment;

            environment = new DefaultEnvironment(_configuration);
            return environment.Probe() ? environment : null;
        }
    }
}