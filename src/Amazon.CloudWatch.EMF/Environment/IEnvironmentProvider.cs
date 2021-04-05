namespace Amazon.CloudWatch.EMF.Environment
{
    public interface IEnvironmentProvider
    {
        IEnvironment ResolveEnvironment();
    }
}