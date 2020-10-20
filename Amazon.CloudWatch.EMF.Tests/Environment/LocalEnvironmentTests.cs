using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Environment
{
    public class LocalEnvironmentTests
    {
        [Fact]
        public void Probe_Returns_False()
        {
            var configuration = new Configuration("", "", "", "", "", Environments.Agent);
            var ctor = new LocalEnvironment(configuration);
            Assert.False(ctor.Probe());
        }
    }
}