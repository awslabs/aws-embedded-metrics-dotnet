using System;
using System.Collections.Generic;
using System.Linq;
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
        private MetricsLogger _metricsLogger;
        private MockSink _sink;
        private IEnvironment _environment;
        private IEnvironmentProvider _environmentProvider;
        private ILoggerFactory _logger;

        public MetricsLoggerTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _logger = _fixture.Create<ILoggerFactory>();
            _environment = _fixture.Create<IEnvironment>();
            _environmentProvider = _fixture.Create<IEnvironmentProvider>();

            _sink = new MockSink();
            _environment.Sink.Returns(_sink);
            _environment.LogGroupName.Returns("LogGroup");
            _environment.Name.Returns("Environment");
            _environment.Type.Returns("Type");
            _environmentProvider.ResolveEnvironment().Returns(_environment);

            _metricsLogger = new MetricsLogger(_environmentProvider, _logger);
        }

        [Fact]
        public void TestPutProperty()
        {
            var propertyName = "Property";
            var propertyValue = "PropValue";
            _metricsLogger.PutProperty(propertyName, propertyValue);
            _metricsLogger.Flush();
            Assert.Equal(propertyValue, _sink.MetricsContext.GetProperty(propertyName));
        }

        [Fact]
        public void TestPutDimension()
        {
            var dimensionName = "dim";
            var dimensionValue = "dimValue";
            _metricsLogger.PutDimensions(new DimensionSet(dimensionName, dimensionValue));
            _metricsLogger.Flush();

            Assert.Single(_sink.MetricsContext.GetAllDimensionSets());
            Assert.Equal(dimensionValue, _sink.MetricsContext.GetAllDimensionSets()[0].GetDimensionValue(dimensionName));
        }

        [Fact]
        public void TestOverrideDefaultDimensions()
        {
            var dimensionName = "dim";
            var dimensionValue = "dimValue";
            var defaultDimName = "defaultDim";
            var defaultDimValue = "defaultDimValue";

            MetricsContext metricsContext = new MetricsContext();
            metricsContext.DefaultDimensions.AddDimension(defaultDimName, defaultDimValue);
            metricsContext.SetDimensions(new DimensionSet(defaultDimName, defaultDimValue));

            _metricsLogger = new MetricsLogger(_environment, metricsContext, _logger);
            _metricsLogger.SetDimensions(new DimensionSet(dimensionName, dimensionValue));
            _metricsLogger.Flush();

            Assert.Single(_sink.MetricsContext.GetAllDimensionSets());
            Assert.Null(_sink.MetricsContext.GetAllDimensionSets()[0].GetDimensionValue(defaultDimName));
        }

        [Fact]
        public void TestOverridePreviousDimensions()
        {
            var dimensionName = "dim";
            var dimensionValue = "dimValue";
            _metricsLogger.PutDimensions(new DimensionSet("foo", "bar"));
            _metricsLogger.SetDimensions(new DimensionSet(dimensionName, dimensionValue));

            _metricsLogger.Flush();

            Assert.Single(_sink.MetricsContext.GetAllDimensionSets());
            Assert.Single(_sink.MetricsContext.GetAllDimensionSets()[0].DimensionKeys);
            Assert.Equal(dimensionValue, _sink.MetricsContext.GetAllDimensionSets()[0].GetDimensionValue(dimensionName));
        }

        [Fact]
        public void SetDimensions_WithUseDefault_PreservesDefaultDimensions()
        {
            string dimensionName = "dim";
            string dimensionValue = "dimValue";
            string defaultDimName = "defaultDim";
            string defaultDimValue = "defaultDimValue";

            MetricsContext metricsContext = new MetricsContext();
            metricsContext.DefaultDimensions.AddDimension(defaultDimName, defaultDimValue);
            _metricsLogger = new MetricsLogger(_environment, metricsContext, _logger);

            _metricsLogger.PutDimensions(new DimensionSet("foo", "bar"));
            _metricsLogger.SetDimensions(true, new DimensionSet(dimensionName, dimensionValue));
            _metricsLogger.Flush();

            Assert.Single(_sink.MetricsContext.GetAllDimensionSets());
            Assert.Equal(2, _sink.MetricsContext.GetAllDimensionSets()[0].DimensionKeys.Count);
            ExpectDimension(dimensionName, dimensionValue);
            ExpectDimension(defaultDimName, defaultDimValue);
        }

        [Fact]
        public void SetDimensions_WithoutUseDefault_DoesNotPreserveDefaultDimensions()
        {
            string dimensionName = "dim";
            string dimensionValue = "dimValue";
            string defaultDimName = "defaultDim";
            string defaultDimValue = "defaultDimValue";

            MetricsContext metricsContext = new MetricsContext();
            metricsContext.DefaultDimensions.AddDimension(defaultDimName, defaultDimValue);
            _metricsLogger = new MetricsLogger(_environment, metricsContext, _logger);

            _metricsLogger.PutDimensions(new DimensionSet("foo", "bar"));
            _metricsLogger.SetDimensions(false, new DimensionSet(dimensionName, dimensionValue));
            _metricsLogger.Flush();

            Assert.Single(_sink.MetricsContext.GetAllDimensionSets());
            Assert.Single(_sink.MetricsContext.GetAllDimensionSets()[0].DimensionKeys);
            ExpectDimension(dimensionName, dimensionValue);
            ExpectDimension(defaultDimName, null);
        }

        [Fact]
        public void TestPutDuplicateDimensions()
        {
            string dimensionName1 = "dim1";
            string dimensionName2 = "dim2";
            string dimensionValue1 = "dimValue1";
            string dimensionValue2 = "dimValue2";
            string dimensionValue3 = "dimValue3";
            string dimensionValue4 = "dimValue4";

            _metricsLogger.PutDimensions(new DimensionSet(dimensionName1, dimensionValue1));
            _metricsLogger.PutDimensions(new DimensionSet(dimensionName2, dimensionValue2));
            _metricsLogger.PutDimensions(new DimensionSet(dimensionName1, dimensionValue3));
            _metricsLogger.PutDimensions(new DimensionSet(dimensionName2, dimensionValue4));
            _metricsLogger.Flush();

            Assert.Equal(4, _sink.MetricsContext.GetAllDimensionSets()[0].DimensionKeys.Count);
            Assert.Equal(dimensionValue3, _sink.MetricsContext.GetAllDimensionSets()[0].GetDimensionValue(dimensionName1));
            Assert.Equal(dimensionValue4, _sink.MetricsContext.GetAllDimensionSets()[0].GetDimensionValue(dimensionName2));
        }

        [Fact]
        public void TestSetPutDuplicateDimensions()
        {
            string dimensionName1 = "dim1";
            string dimensionName2 = "dim2";
            string dimensionName3 = "dim3";
            string dimensionValue1 = "dimValue1";
            string dimensionValue2 = "dimValue2";
            string dimensionValue3 = "dimValue3";
            string dimensionValue4 = "dimValue4";

            _metricsLogger.PutDimensions(new DimensionSet(dimensionName1, dimensionValue1));
            _metricsLogger.SetDimensions(new DimensionSet(dimensionName2, dimensionValue1));
            _metricsLogger.PutDimensions(new DimensionSet(dimensionName3, dimensionValue2));
            _metricsLogger.PutDimensions(new DimensionSet(dimensionName2, dimensionValue3));
            _metricsLogger.PutDimensions(new DimensionSet(dimensionName3, dimensionValue4));
            _metricsLogger.Flush();

            Assert.Equal(2, _sink.MetricsContext.GetAllDimensionSets().Count);
            Assert.Equal(dimensionValue3, _sink.MetricsContext.GetAllDimensionSets()[0].GetDimensionValue(dimensionName2));
            Assert.Equal(dimensionValue4, _sink.MetricsContext.GetAllDimensionSets()[1].GetDimensionValue(dimensionName3));
        }

        [Theory]
        [InlineData(true, 2)]
        [InlineData(false, 1)]
        public void ResetDimensions_ClearsDimensions(bool useDefault, int expectedDimensionSets)
        {
            string dimensionName1 = "dim";
            string dimensionValue1 = "dimValue";
            string dimensionName2 = "dim2";
            string dimensionValue2 = "dimValue2";

            MetricsContext metricsContext = new MetricsContext();
            metricsContext.DefaultDimensions.AddDimension("foo", "bar");
            _metricsLogger = new MetricsLogger(_environment, metricsContext, _logger);

            _metricsLogger.PutDimensions(new DimensionSet(dimensionName1, dimensionValue1));
            _metricsLogger.ResetDimensions(useDefault);
            _metricsLogger.PutDimensions(new DimensionSet(dimensionName2, dimensionValue2));
            _metricsLogger.Flush();

            Assert.Single(_sink.MetricsContext.GetAllDimensionSets());
            Assert.Equal(expectedDimensionSets, _sink.MetricsContext.GetAllDimensionSets()[0].DimensionKeys.Count);
            Assert.Null(_sink.MetricsContext.GetAllDimensionSets()[0].GetDimensionValue(dimensionName1));
            ExpectDimension(dimensionName2, dimensionValue2);
            ExpectDimension(dimensionName1, null);
        }

        [Fact]
        public void TestSetNameSpace()
        {
            string namespaceValue = "TestNamespace";
            _metricsLogger.SetNamespace(namespaceValue);
            _metricsLogger.Flush();
            Assert.Equal(namespaceValue, _sink.MetricsContext.Namespace);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("namespace ")]
        [InlineData("ǹẚḿⱸṥṕấćē")]
        [InlineData("name$pace")]
        public void SetNamespace_WithInvalidNamespace_ThrowsInvalidNamespaceException(string namespaceValue)
        {
            Assert.Throws<InvalidNamespaceException>(() => _metricsLogger.SetNamespace(namespaceValue));
        }

        [Fact]
        public void SetNamespace_WithNameTooLong_ThrowsInvalidNamespaceException()
        {
            string namespaceValue = new string('a', Constants.MAX_NAMESPACE_LENGTH + 1);
            Assert.Throws<InvalidNamespaceException>(() => _metricsLogger.SetNamespace(namespaceValue));
        }

        [Fact]
        public void TestFlushWithConfiguredServiceName()
        {
            string serviceName = "TestServiceName";
            _environment.Name.Returns(serviceName);
            _metricsLogger.Flush();

            ExpectDimension("ServiceName", serviceName);
        }

        [Fact]
        public void TestFlushWithConfiguredServiceType()
        {
            string serviceType = "TestServiceType";
            _environment.Type.Returns(serviceType);
            _metricsLogger.Flush();

            ExpectDimension("ServiceType", serviceType);
        }

        [Fact]
        public void TestFlushWithDefaultDimensionDefined()
        {
            MetricsContext metricsContext = new MetricsContext();
            metricsContext.DefaultDimensions.Dimensions.Add("foo", "bar");
            _metricsLogger = new MetricsLogger(_environment, metricsContext, _logger);
            string logGroup = "TestLogGroup";
            _environment.LogGroupName.Returns(logGroup);
            _metricsLogger.Flush();

            ExpectDimension("foo", "bar");
            ExpectDimension("LogGroup", null);
        }

        [Fact]
        public void SetFlushPreserveDimensions_ToFalse_DoesNotPreserveDimensions()
        {
            string dimensionName = "dim";
            string dimensionValue = "val";
            string defaultDim = "defaultDim";
            string defaultDimValue = "defaultDimValue";

            MetricsContext metricsContext = new MetricsContext();
            metricsContext.DefaultDimensions.AddDimension(defaultDim, defaultDimValue);
            MetricsLogger _metricsLogger1 = new MetricsLogger(_environment, metricsContext, _logger);

            _metricsLogger1.PutDimensions(new DimensionSet(dimensionName, dimensionValue));
            _metricsLogger1.FlushPreserveDimensions = false;
            _metricsLogger1.Flush();

            Assert.Equal(2, _sink.MetricsContext.GetAllDimensionSets()[0].DimensionKeys.Count);
            ExpectDimension(dimensionName, dimensionValue);
            ExpectDimension(defaultDim, defaultDimValue);

            _metricsLogger1.Flush();

            Assert.Single(_sink.MetricsContext.GetAllDimensionSets()[0].DimensionKeys);
            ExpectDimension(dimensionName, null);
            ExpectDimension(defaultDim, defaultDimValue);
        }

        [Fact]
        public void TestPutMetricWithNoUnit()
        {
            string expectedKey = "test";
            double expectedValue = 2.0;
            _metricsLogger.PutMetric(expectedKey, expectedValue);
            _metricsLogger.Flush();

            var metricDirective = typeof(MetricsContext)
                .GetField("_metricDirective", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_sink.MetricsContext) as MetricDirective;

            var metricDefinition = metricDirective.Metrics.FirstOrDefault(m => m.Name == expectedKey);

            Assert.Equal(expectedValue, metricDefinition.Values[0]);
            Assert.Equal(Unit.NONE, metricDefinition.Unit);
            Assert.Equal(StorageResolution.STANDARD, metricDefinition.StorageResolution);
        }

        [Fact]
        public void TestPutMetricWithUnit()
        {
            string expectedKey = "test";
            double expectedValue = 2.0;
            _metricsLogger.PutMetric(expectedKey, expectedValue, Unit.MILLISECONDS);
            _metricsLogger.Flush();

            var metricDirective = typeof(MetricsContext)
              .GetField("_metricDirective", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .GetValue(_sink.MetricsContext) as MetricDirective;

            var metricDefinition = metricDirective.Metrics.FirstOrDefault(m => m.Name == expectedKey);

            Assert.Equal(expectedValue, metricDefinition.Values[0]);
            Assert.Equal(Unit.MILLISECONDS, metricDefinition.Unit);
            Assert.Equal(StorageResolution.STANDARD, metricDefinition.StorageResolution);
        }

        [Fact]
        public void TestPutMetricWithHighResolutionCheck()
        {
            string expectedKey = "test";
            double expectedValue = 2.0;
            _metricsLogger.PutMetric(expectedKey, expectedValue, Unit.MILLISECONDS, StorageResolution.HIGH);
            _metricsLogger.Flush();

            var metricDirective = typeof(MetricsContext)
              .GetField("_metricDirective", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .GetValue(_sink.MetricsContext) as MetricDirective;

            var metricDefinition = metricDirective.Metrics.FirstOrDefault(m => m.Name == expectedKey);
            Assert.Equal(StorageResolution.HIGH, metricDefinition.StorageResolution);
        }

        [Fact]
        public void TestPutMetricWithStandardResolutionCheck()
        {
            string expectedKey = "test";
            double expectedValue = 2.0;
            _metricsLogger.PutMetric(expectedKey, expectedValue, Unit.MILLISECONDS, StorageResolution.STANDARD);
            _metricsLogger.Flush();

            var metricDirective = typeof(MetricsContext)
              .GetField("_metricDirective", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .GetValue(_sink.MetricsContext) as MetricDirective;

            var metricDefinition = metricDirective.Metrics.FirstOrDefault(m => m.Name == expectedKey);
            Assert.Equal(StorageResolution.STANDARD, metricDefinition.StorageResolution);
        }

        [Theory]
        [InlineData(null, 1)]
        [InlineData("", 1)]
        [InlineData(" ", 1)]
        [InlineData("metric", Double.PositiveInfinity)]
        [InlineData("metric", Double.NegativeInfinity)]
        [InlineData("metric", Double.NaN)]
        public void PutMetric_WithInvalidMetric_ThrowsInvalidMetricException(string metricName, double metricValue)
        {
            Assert.Throws<InvalidMetricException>(() => _metricsLogger.PutMetric(metricName, metricValue, Unit.NONE));
        }

        [Fact]
        public void PutMetric_WithNameTooLong_ThrowsInvalidMetricException()
        {
            string metricName = new string('a', Constants.MAX_METRIC_NAME_LENGTH + 1);
            Assert.Throws<InvalidMetricException>(() => _metricsLogger.PutMetric(metricName, 1, Unit.NONE));
        }

        [Fact]
        public void PutMetric_WithSameMetricHavingDifferentResolution_ThrowsInvalidMetricException()
        {
            _metricsLogger.PutMetric("TestMetric", 1, Unit.COUNT, StorageResolution.STANDARD);
            Assert.Throws<InvalidMetricException>(() => _metricsLogger.PutMetric("TestMetric", 1, Unit.COUNT, StorageResolution.HIGH));
        }


        [Fact]
        public void PutMetric_WithSameMetricHavingDifferentResolutionInDifferentFlush_TheAllow()
        {
            _metricsLogger.PutMetric("TestMetric", 1, Unit.COUNT, StorageResolution.STANDARD);
            _metricsLogger.Flush();

            _metricsLogger.PutMetric("TestMetric", 1, Unit.COUNT, StorageResolution.HIGH);
            _metricsLogger.Flush();

            var metricDirective = typeof(MetricsContext)
              .GetField("_metricDirective", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .GetValue(_sink.MetricsContext) as MetricDirective;

            Assert.True(metricDirective.Metrics.Count() == 1);

            var metricDefinition = metricDirective.Metrics.FirstOrDefault(m => m.Name == "TestMetric");
            Assert.Equal(StorageResolution.HIGH, metricDefinition.StorageResolution);
        }


        [Fact]
        public void TestPutMetaData()
        {
            string expectedKey = "testKey";
            string expectedValue = "testValue";
            _metricsLogger.PutMetadata(expectedKey, expectedValue);
            _metricsLogger.Flush();

            var rootNode = typeof(MetricsContext)
                .GetField("_rootNode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_sink.MetricsContext) as RootNode;

            rootNode.AWS.CustomMetadata.TryGetValue(expectedKey, out var actualValue);

            Assert.Equal(expectedValue, actualValue);
        }

        private void ExpectDimension(string dimension, string value)
        {
            List<DimensionSet> dimensions = _sink.MetricsContext.GetAllDimensionSets();

            Assert.Single(dimensions);

            if (value != null)
            {
                Assert.Equal(value, dimensions[0].GetDimensionValue(dimension));
            }
            else
                Assert.Null(dimensions[0].GetDimensionValue(dimension));
        }
    }
}
