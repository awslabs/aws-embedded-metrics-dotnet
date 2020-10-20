namespace Amazon.CloudWatch.EMF.Environment
{
    public interface IEnvironmentProvider
    {
        IEnvironment DefaultEnvironment { get; }

        IEnvironment ResolveEnvironment();
    }
}