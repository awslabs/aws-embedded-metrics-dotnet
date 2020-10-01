using Amazon.CloudWatch.EMF.Environment;
using NFluent;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Environment
{
    public class DefaultEnvironmentTests
    {
        [Fact]
        public void Probe_Returns_True()
        {
            var ctor = new DefaultEnvironment();
            Check.That(ctor.Probe().Equals(true));
        }
    }
}