using System;
using System.Collections.Generic;
using System.Linq;
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

            Assert.Equal("Value", rootNode.GetProperties()["Property"]);
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

            Assert.Equal("DefaultDimValue", emfMap["DefaultDim"]);
            Assert.Equal("us-east-1", emfMap["Region"]);

            Assert.Equal(10.0, emfMap["Count"]);
            Assert.Equal("PropertyValue", emfMap["Property"]);
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

        [Fact]
        public void Serialize_MoreThan100DataPoints()
        {
            MetricsContext mc = new MetricsContext();
            const int expectedEmfLogs = 2;
            const int dataPointCount = 102;

            mc.DefaultDimensions.AddDimension("DefaultDim", "DefaultDimValue");
            mc.PutDimension("Region", "us-east-1");
            for (var i = 0; i < dataPointCount; i++)
            {
                mc.PutMetric("Count", i);
            }

            mc.PutProperty("Property", "PropertyValue");
            mc.Namespace = "test-namespace";

            List<string> emfLogs = mc.Serialize();
            Assert.Equal(expectedEmfLogs, emfLogs.Count);

            List<IList<Double>> allMetricValues = new List<IList<Double>>();
            foreach (var emfLog in emfLogs)
            {
                var emfMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(emfLog);
                var metricValues = JsonConvert.DeserializeObject<List<Double>>(emfMap["Count"].ToString());
                allMetricValues.Add(metricValues);

                Assert.Equal("DefaultDimValue", emfMap["DefaultDim"]);
                Assert.Equal("us-east-1", emfMap["Region"]);
                Assert.Equal("PropertyValue", emfMap["Property"]);

                var metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(emfMap["_aws"].ToString());
                Assert.True(metadata.ContainsKey("Timestamp"));
                Assert.True(metadata.ContainsKey("CloudWatchMetrics"));
            }

            List<Double> expectedValues = new List<Double>();
            for (int i = 0; i < dataPointCount; i++)
            {
                expectedValues.Add(i);
            }

            Assert.Equal(expectedValues.Take(Constants.MAX_DATAPOINTS_PER_METRIC), allMetricValues[0] as List<Double>);
            Assert.Equal(expectedValues.Skip(Constants.MAX_DATAPOINTS_PER_METRIC), allMetricValues[1] as List<Double>);
        }

        [Fact]
        public void Serialize_MoreThan100Metrics()
        {
            MetricsContext mc = new MetricsContext();
            const int expectedEmfLogs = 2;
            const int metricCount = 101;

            mc.DefaultDimensions.AddDimension("DefaultDim", "DefaultDimValue");
            mc.PutDimension("Region", "us-east-1");
            for (var i = 0; i < metricCount; i++)
            {
                mc.PutMetric("Count" + i, i);
            }

            mc.PutProperty("Property", "PropertyValue");
            mc.Namespace = "test-namespace";

            List<string> emfLogs = mc.Serialize();
            Assert.Equal(expectedEmfLogs, emfLogs.Count);

            List<List<string>> allMetrics = new List<List<string>>();
            for (int i = 0; i < expectedEmfLogs; i++)
            {
                var emfMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(emfLogs[i]);

                allMetrics.Add(emfMap.Where(pair => pair.Key.Contains("Count")).Select(pair => pair.Key).ToList());

                Assert.Equal("DefaultDimValue", emfMap["DefaultDim"]);
                Assert.Equal("us-east-1", emfMap["Region"]);
                Assert.Equal("PropertyValue", emfMap["Property"]);

                var metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(emfMap["_aws"].ToString());
                Assert.True(metadata.ContainsKey("Timestamp"));
                Assert.True(metadata.ContainsKey("CloudWatchMetrics"));
            }

            Assert.Equal(Constants.MAX_METRICS_PER_EVENT, allMetrics[0].Count);
            for (var i = 0; i < Constants.MAX_METRICS_PER_EVENT; i++)
            {
                Assert.Contains("Count" + i, allMetrics[0]);
            }

            Assert.Single(allMetrics[1]);
            Assert.Contains("Count100", allMetrics[1]);
        }
    }
}