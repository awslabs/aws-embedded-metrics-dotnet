using Amazon.CloudWatch.EMF.Environment;
using Amazon.CloudWatch.EMF.Logger;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Tests.Sink;
using Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Amazon.CloudWatch.EMF.Tests.Logger
{
    public class MetricsLoggerTests
    {
        private readonly IFixture _fixture;
        private MetricsLogger _logger;
        private MockSink _sink;

        public MetricsLoggerTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            var logger = _fixture.Create<ILoggerFactory>();
            var env = _fixture.Create<IEnvironment>();
            var environmentProvider = _fixture.Create<IEnvironmentProvider>();

            _sink = new MockSink();
            env.Sink.Returns(_sink);
            environmentProvider.ResolveEnvironment().Returns(env);
            
            _logger = new MetricsLogger(environmentProvider, logger);
        }
        
        [Fact]
        public void PutProperty()
        {
            var propertyName = "Property";
            var propertyValue = "PropValue";
            _logger.PutProperty(propertyName, propertyValue);
            _logger.Flush();
            Assert.Equal(propertyValue, _sink.MetricsContext.GetProperty(propertyName));
        }
        
        [Fact]
        public void PutDimension()
        {
            var dimensionName = "dim";
            var dimensionValue = "dimValue";
            _logger.PutDimensions(new DimensionSet(dimensionName, dimensionValue));
            _logger.Flush();

            string actualValue = string.Empty;
            foreach (var ds in _sink.MetricsContext.GetAllDimensionSets())
            {
                if (ds.DimensionKeys.Contains(dimensionName))
                    actualValue = ds.GetDimensionValue(dimensionName);
            }
            Assert.Equal(dimensionValue, actualValue);
        }
    }
}
