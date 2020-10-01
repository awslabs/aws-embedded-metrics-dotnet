using Amazon.CloudWatch.EMF.Environment;
using NFluent;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Environment
{
    public class LambdaEnvironmentTests
    {
        [Fact]
        public void Probe_Returns_True()
        {
            var ctor = new LambdaEnvironment();
            Check.That(ctor.Probe().Equals(true));
        }
    }
}