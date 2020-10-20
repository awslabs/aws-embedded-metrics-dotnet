using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.CloudWatch.EMF.Config;

namespace Amazon.CloudWatch.EMF.Environment
{
    /// <summary>
    /// A provider that will detect the environment.
    /// </summary>
    public class EnvironmentProvider
    {
        private static readonly IConfiguration _configuration = EnvironmentConfigurationProvider.Config;
        private static IEnvironment _cachedEnvironment;
        private readonly IEnvironment _lambdaEnvironment = new LambdaEnvironment();
        private readonly IEnvironment _ec2Environment = new EC2Environment(_configuration, new ResourceFetcher());
        private readonly IEnvironment _ecsEnvironment = new ECSEnvironment(_configuration, new ResourceFetcher());

        private IEnvironment[] _allEnvironments;

        internal IEnvironment DefaultEnvironment { get; } = new DefaultEnvironment(_configuration);

        public EnvironmentProvider()
        {
            // Ordering of this array matters
            _allEnvironments = new IEnvironment[] { _lambdaEnvironment, _ecsEnvironment, _ec2Environment, DefaultEnvironment };
        }

        /// <summary>
        ///  Find the current environment
        /// </summary>
        /// <returns></returns>
        internal Task<IEnvironment> ResolveEnvironment()
        {
            if (_cachedEnvironment != null)
                return Task.FromResult(_cachedEnvironment);

            IEnvironment env = GetEnvironmentFromOverride();
            if (env != null)
            {
                _cachedEnvironment = env;
                return Task.FromResult(_cachedEnvironment);
            }

            foreach (IEnvironment environment in _allEnvironments)
            {
                if (environment.Probe())
                {
                    _cachedEnvironment = environment;
                    return Task.FromResult(_cachedEnvironment);
                }
            }

            return Task.FromResult(DefaultEnvironment);
        }

        private IEnvironment GetEnvironmentFromOverride()
        {
            IConfiguration config = EnvironmentConfigurationProvider.Config;

            IEnvironment environment;
            switch (config.EnvironmentOverride)
            {
                case Environments.Lambda:
                    environment = _lambdaEnvironment;
                    break;
                case Environments.Agent:
                    environment = DefaultEnvironment;
                    break;
                case Environments.EC2:
                    environment = _ec2Environment;
                    break;
                case Environments.ECS:
                    environment = _ecsEnvironment;
                    break;
                case Environments.Local:
                    environment = new LocalEnvironment(_configuration);
                    break;
                case Environments.Unknown:
                    environment = null;
                    break;
                default:
                    environment = null;
                    break;
            }

            return environment;
        }
    }
}