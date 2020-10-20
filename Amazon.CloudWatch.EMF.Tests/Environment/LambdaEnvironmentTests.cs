using Amazon.CloudWatch.EMF.Environment;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Environment
{
    public class LambdaEnvironmentTests
    {
        [Fact]
        public void Probe_Returns_False()
        {
            var ctor = new LambdaEnvironment();
            Assert.False(ctor.Probe());
        }
    }
}