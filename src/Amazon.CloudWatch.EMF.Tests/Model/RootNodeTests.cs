using System.Collections.Generic;
using Amazon.CloudWatch.EMF.Model;
using Newtonsoft.Json;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Model
{
    public class RootNodeTests
    {
        [Fact]
        public void PutProperty_SavesKeyValue()
        {
            RootNode rootNode = new RootNode();
            rootNode.PutProperty("Property", "Value");

            Assert.Equal( "Value",rootNode.GetProperties()["Property"]);
        }

        [Fact]
        public void Serialize_Returns_ValidValues()
        {
            MetricsContext mc = new MetricsContext();

            mc.DefaultDimensions.AddDimension("DefaultDim", "DefaultDimValue");
            mc.PutDimension("Region", "us-east-1");
            mc.PutMetric("Count", 10);
            mc.PutProperty("Property", "PropertyValue");
            mc.Namespace = "test-namespace";

            List<string> emfLogs = mc.Serialize();
            
            var emfMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(emfLogs[0]);

            Assert.Equal( "DefaultDimValue", emfMap["DefaultDim"]);
            Assert.Equal( "us-east-1", emfMap["Region"]);

            Assert.Equal( 10.0, emfMap["Count"]);
            Assert.Equal( "PropertyValue", emfMap["Property"]);
            var metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(emfMap["_aws"].ToString());
            Assert.True(metadata.ContainsKey("Timestamp"));
            Assert.True(metadata.ContainsKey("CloudWatchMetrics"));
        }

        [Fact]
        public void MetadataNode_IsNotSerialize_IfNoMetrics()
        {
            // arrange
            RootNode rootNode = new RootNode();

            // act
            var json = JsonConvert.SerializeObject(rootNode);

            // assert
            Assert.DoesNotContain("_aws", json);
        }

        [Fact]
        public void MetadataNode_IsSerialized_IfMetricsArePresent()
        {
            // arrange
            RootNode rootNode = new RootNode();
            var metricDirective = new MetricDirective();
            metricDirective.PutMetric("Metric", 1);
            rootNode.AWS.CloudWatchMetrics.Add(metricDirective);

            // act
            var json = JsonConvert.SerializeObject(rootNode);

            // assert
            Assert.Contains("_aws", json);
        }
    }
}