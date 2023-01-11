using System.Collections.Generic;
using Amazon.CloudWatch.EMF.Model;
using Newtonsoft.Json;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Model
{
    public class MetricDirectiveTests
    {
        [Fact]
        public void MetricDirective_DefaultNameSpace_Returns_ValidJson()
        {
            MetricDirective metricDirective = new MetricDirective();

            string jsonString = JsonConvert.SerializeObject(metricDirective);

            Assert.Equal("{\"Namespace\":\"aws-embedded-metrics\",\"Metrics\":[],\"Dimensions\":[[]]}", jsonString);

        }

        [Fact]
        public void MetricDirective_SetNameSpace_Returns_ValidJson()
        {
            MetricDirective metricDirective = new MetricDirective();
            metricDirective.Namespace = "test-lambda-metrics";

            string jsonString = JsonConvert.SerializeObject(metricDirective);

            Assert.Equal("{\"Namespace\":\"test-lambda-metrics\",\"Metrics\":[],\"Dimensions\":[[]]}", jsonString);

        }


        [Fact]
        public void MetricDirective_PutMetric_Returns_ValidJson()
        {
            MetricDirective metricDirective = new MetricDirective();
            metricDirective.PutMetric("Time", 10);

            string jsonString = JsonConvert.SerializeObject(metricDirective);

            Assert.Equal("{\"Namespace\":\"aws-embedded-metrics\",\"Metrics\":[{\"Name\":\"Time\",\"Unit\":\"None\"}],\"Dimensions\":[[]]}", jsonString);

        }

        [Fact]
        public void MetricDirective_PutSameMetricMultipleTimes_Returns_ValidValues()
        {
            MetricDirective metricDirective = new MetricDirective();
            metricDirective.PutMetric("Time", 10);
            metricDirective.PutMetric("Time", 20);

            Assert.Single(metricDirective.Metrics);
            Assert.Equal(new List<double>() { 10, 20 }, metricDirective.Metrics[0].Values);
        }

        [Fact]
        public void MetricDirective_AddDimensions_Returns_ValidJson()
        {
            MetricDirective metricDirective = new MetricDirective();
            DimensionSet ds = new DimensionSet("Region", "US-West-2");
            ds.AddDimension("Instance", "instance-1");
            metricDirective.CustomDimensionSets.Add(ds);

            string jsonString = JsonConvert.SerializeObject(metricDirective);

            Assert.Equal("{\"Namespace\":\"aws-embedded-metrics\",\"Metrics\":[],\"Dimensions\":[[\"Region\",\"Instance\"]]}", jsonString);
        }

        [Fact]
        public void MetricDirective_AddDimensions_AddRange_Returns_ValidJson()
        {
            MetricDirective metricDirective = new MetricDirective();
            DimensionSet ds = new DimensionSet("Region", "US-West-2");
            ds.AddRange(new DimensionSet("Instance", "instance-1"));
            metricDirective.CustomDimensionSets.Add(ds);

            string jsonString = JsonConvert.SerializeObject(metricDirective);

            Assert.Equal("{\"Namespace\":\"aws-embedded-metrics\",\"Metrics\":[],\"Dimensions\":[[\"Region\",\"Instance\"]]}", jsonString);
        }

        [Fact]
        public void MetricDirective_PutMultipleDimensions_Returns_ValidJson()
        {
            MetricDirective metricDirective = new MetricDirective();
            metricDirective.CustomDimensionSets.Add(new DimensionSet("Region", "US-West-2"));
            metricDirective.CustomDimensionSets.Add(new DimensionSet("Instance", "instance-1"));

            string jsonString = JsonConvert.SerializeObject(metricDirective);

            Assert.Equal("{\"Namespace\":\"aws-embedded-metrics\",\"Metrics\":[],\"Dimensions\":[[\"Region\",\"Instance\"]]}", jsonString);
        }

        [Fact]
        public void MetricDirective_PutMetricHighResolution_Returns_ValidJson()
        {
            MetricDirective metricDirective = new MetricDirective();
            metricDirective.PutMetric("Time", 10,StorageResolution.HIGH);

            string jsonString = JsonConvert.SerializeObject(metricDirective);

            Assert.Equal("{\"Namespace\":\"aws-embedded-metrics\",\"Metrics\":[{\"Name\":\"Time\",\"Unit\":\"None\",\"StorageResolution\":1}],\"Dimensions\":[[]]}", jsonString);

        }
    }
}