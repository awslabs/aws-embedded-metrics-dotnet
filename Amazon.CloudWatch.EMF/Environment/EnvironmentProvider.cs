using System.Threading.Tasks;

namespace Amazon.CloudWatch.EMF.Environment
{
    public class EnvironmentProvider
    {
        public Task<IEnvironment> ResolveEnvironment()
        {
            return Task.FromResult(DefaultEnvironment);
        }

        public IEnvironment DefaultEnvironment { get; set; } = new DefaultEnvironment();
    }
}