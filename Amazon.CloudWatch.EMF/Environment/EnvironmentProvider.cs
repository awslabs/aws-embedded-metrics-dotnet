using System;
using System.Threading.Tasks;
using Amazon.CloudWatch.EMF.Config;

namespace Amazon.CloudWatch.EMF.Environment
{
    /// <summary>
    /// A provider that will detect the environment.
    /// </summary>
    public class EnvironmentProvider
    {
        private static IConfiguration _configuration;
        private static IResourceFetcher _resourceFetcher;
        private static IEnvironment _cachedEnvironment;

        internal IEnvironment DefaultEnvironment
        {
            get
            {
                return new DefaultEnvironment(_configuration);
            }
        }

        public EnvironmentProvider(IConfiguration configuration, IResourceFetcher resourceFetcher)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _resourceFetcher = resourceFetcher ?? throw new ArgumentNullException(nameof(resourceFetcher));
        }

        /// <summary>
        ///  Find the current environment
        /// </summary>
        /// <returns></returns>
        internal Task<IEnvironment> ResolveEnvironment()
        {
            if (_cachedEnvironment != null)
                return Task.FromResult(_cachedEnvironment);

            var env = GetEnvironmentFromConfig();
            if (env != null)
            {
                _cachedEnvironment = env;
                return Task.FromResult(_cachedEnvironment);
            }

            env = GetEnvironmentByProbe();
            if (env != null)
            {
                _cachedEnvironment = env;
                return Task.FromResult(_cachedEnvironment);
            }

            return Task.FromResult(DefaultEnvironment);
        }

        private IEnvironment GetEnvironmentFromConfig()
        {
            IEnvironment environment;
            switch (_configuration.EnvironmentOverride)
            {
                case Environments.Lambda:
                    environment = new LambdaEnvironment();
                    break;
                case Environments.Agent:
                    environment = new DefaultEnvironment(_configuration);
                    break;
                case Environments.EC2:
                    environment = new EC2Environment(_configuration, _resourceFetcher);
                    break;
                case Environments.ECS:
                    environment = new ECSEnvironment(_configuration, _resourceFetcher);
                    break;
                case Environments.Local:
                    environment = new LocalEnvironment(_configuration);
                    break;
                default:
                    environment = null;
                    break;
            }

            return environment;
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