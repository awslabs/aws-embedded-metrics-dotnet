using System.Threading.Tasks;
using Amazon.CloudWatch.EMF.Config;

namespace Amazon.CloudWatch.EMF.Environment
{
    /// <summary>
    /// A provider that will detect the environment.
    /// </summary>
    public class EnvironmentProvider
    {
        private static IEnvironment _cachedEnvironment;
        private static readonly Configuration _configuration = EnvironmentConfigurationProvider.Config;
        private readonly IEnvironment _lambdaEnvironment = new LambdaEnvironment();
        //private readonly IEnvironment ec2Environment = new EC2Environment(_configuration, new ResourceFetcher());
        //private readonly IEnvironment ecsEnvironment = new ECSEnvironment(_configuration, new ResourceFetcher());
            
        // Ordering of this array matters
        //_lambdaEnvironment, ecsEnvironment, ec2Environment, _defaultEnvironment
        //private IEnvironment[] iEnvironments = {_lambdaEnvironment, _defaultEnvironment };
        
        internal IEnvironment DefaultEnvironment { get; } = new LambdaEnvironment();
        /// <summary>
        /// 
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
            
            return Task.FromResult(DefaultEnvironment);
        }


        
        private IEnvironment GetEnvironmentFromOverride()
        {
            Configuration config = EnvironmentConfigurationProvider.Config;

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
                    environment = DefaultEnvironment;
                    break;
                case Environments.ECS:
                    environment = DefaultEnvironment;
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