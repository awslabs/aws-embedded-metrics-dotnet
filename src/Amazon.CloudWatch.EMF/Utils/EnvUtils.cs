namespace Amazon.CloudWatch.EMF.Utils
{
    public class EnvUtils
    {
        internal static string GetEnv(string name)
        {
            return System.Environment.GetEnvironmentVariable(name);
        }
    }
}