using System.Threading.Tasks;

namespace Amazon.CloudWatch.EMF.Environment
{
    public interface IEnvironmentProvider
    {
        IEnvironment DefaultEnvironment { get; }

        Task<IEnvironment> ResolveEnvironment();
    }
}