using System.Collections.Generic;
using Amazon.CloudWatch.EMF.Model;
using Newtonsoft.Json;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Model
{
    public class MetricDefinitionTests
    {
        [Fact]
        public void MetricDefinition_WithoutUnit_Returns_ValidJson()
        {
            MetricDefinition metricDefinition = new MetricDefinition("Time");

            string metricString = JsonConvert.SerializeObject(metricDefinition);

            Assert.Equal("{\"Name\":\"Time\",\"Unit\":\"None\"}", metricString);
            Assert.Empty(metricDefinition.Values);
        }

        [Fact]
        public void MetricDefinition_WithValue_Returns_ValidJson()
        {
            MetricDefinition metricDefinition = new MetricDefinition("Time", 20);

            string metricString = JsonConvert.SerializeObject(metricDefinition);

            Assert.Equal("{\"Name\":\"Time\",\"Unit\":\"None\"}", metricString);
            Assert.Equal(new List<double>() { 20 }, metricDefinition.Values);
        }

        [Fact]
        public void MetricDefinition_WithUnitAndValue_Returns_ValidJson()
        {
            MetricDefinition metricDefinition = new MetricDefinition("Time", 10, Unit.MILLISECONDS);

            string metricString = JsonConvert.SerializeObject(metricDefinition);

            Assert.Equal("{\"Name\":\"Time\",\"Unit\":\"Milliseconds\"}", metricString);
            Assert.Equal(new List<double>() { 10 }, metricDefinition.Values);
        }

        [Fact]
        public void MetricDefinition_AddValue_Returns_ValidValues()
        {
            MetricDefinition metricDefinition = new MetricDefinition("Time", 10, Unit.MILLISECONDS);

            Assert.Equal(new List<double>() { 10 }, metricDefinition.Values);

            metricDefinition.AddValue(20);

            Assert.Equal(new List<double>() { 10, 20 }, metricDefinition.Values);
        }

        [Fact]
        public void MetricDefinition_WithHighResolution_Returns_ValidJson()
        {
             MetricDefinition metricDefinition = new MetricDefinition("Time", 10, Unit.MILLISECONDS,StorageResolution.HIGH);
             
             string metricString = JsonConvert.SerializeObject(metricDefinition);
             
             Assert.Equal("{\"Name\":\"Time\",\"Unit\":\"Milliseconds\",\"StorageResolution\":\"1\"}", metricString);
        }
    }
}