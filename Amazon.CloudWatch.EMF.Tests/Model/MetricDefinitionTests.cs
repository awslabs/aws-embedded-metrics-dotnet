using System.Collections.Generic;
using Amazon.CloudWatch.EMF.Model;
using Newtonsoft.Json;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Model
{
    public class MetricDefinitionTests
    {
        [Fact]
        public void MetricDefinition_WithoutUnit()
        {
            MetricDefinition metricDefinition = new MetricDefinition("Time");

            string metricString = JsonConvert.SerializeObject(metricDefinition);

            Assert.Equal("{\"Name\":\"Time\",\"Unit\":\"None\"}", metricString);
        }
        
        [Fact]
        public void MetricDefinition_WithUnit()
        {
            MetricDefinition metricDefinition = new MetricDefinition("Time", Unit.MILLISECONDS, 10);

            string metricString = JsonConvert.SerializeObject(metricDefinition);

            Assert.Equal( "{\"Name\":\"Time\",\"Unit\":\"Milliseconds\"}", metricString);
        }
        
        [Fact]
        public void MetricDefinition_AddValue()
        {
            MetricDefinition metricDefinition = new MetricDefinition("Time", Unit.MILLISECONDS, 10);
            
            Assert.Equal(new List<double>(){10}, metricDefinition.Values);

            metricDefinition.AddValue(20);
            
            Assert.Equal(new List<double>(){10,20}, metricDefinition.Values);

        }
    }
}