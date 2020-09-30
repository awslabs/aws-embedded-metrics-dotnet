using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;

namespace Amazon.CloudWatch.EMF.Environment
{
    public class DefaultEnvironment : IEnvironment
    {
        public string Name => throw new System.NotImplementedException();

        public string Type => throw new System.NotImplementedException();

        public string LogGroupName => throw new System.NotImplementedException();

        public ISink Sink => throw new System.NotImplementedException();

        public void ConfigureContext(MetricsContext context)
        {
            throw new System.NotImplementedException();
        }

        public bool Probe()
        {
            throw new System.NotImplementedException();
        }
    }
}