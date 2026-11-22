using System;
using System.Linq;
using Amazon.CloudWatch.EMF.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Model
{
    // JsonConvert.DefaultSettings is shared by all test collections in this process.
    [CollectionDefinition("JsonConvertDefaultSettings", DisableParallelization = true)]
    public class JsonConvertDefaultSettingsCollection
    {
    }

    [Collection("JsonConvertDefaultSettings")]
    public class MetadataSerializationTests : IDisposable
    {
        private readonly Func<JsonSerializerSettings> _previousSettings = JsonConvert.DefaultSettings;

        [Theory]
        [InlineData("default", 1)]
        [InlineData("camel", 1)]
        [InlineData("snake", 1)]
        [InlineData("default", 101)]
        [InlineData("camel", 101)]
        [InlineData("snake", 101)]
        public void Serialize_PreservesMetadataNames(string namingStrategy, int metricCount)
        {
            ConfigureNamingStrategy(namingStrategy);
            var context = new MetricsContext();
            var timestamp = DateTime.UtcNow;
            context.SetTimestamp(timestamp);
            context.Namespace = "test-namespace";
            context.PutDimension("ServiceName", "test-service");
            context.PutMetadata("CustomMetadata", "custom-value");
            for (int i = 0; i < metricCount; i++)
            {
                context.PutMetric("Metric" + i, i);
            }

            var logs = context.Serialize();

            Assert.Equal((metricCount + 99) / 100, logs.Count);
            var serializedMetrics = 0;
            foreach (var log in logs)
            {
                var document = JObject.Parse(log);
                var metadata = Assert.IsType<JObject>(document["_aws"]);
                Assert.Equal(
                    new[] { "CloudWatchMetrics", "CustomMetadata", "Timestamp" },
                    metadata.Properties().Select(property => property.Name).OrderBy(name => name));
                Assert.Equal(new DateTimeOffset(timestamp).ToUnixTimeMilliseconds(), metadata["Timestamp"].Value<long>());
                Assert.Equal("custom-value", metadata["CustomMetadata"].Value<string>());
                Assert.Equal("test-service", document["ServiceName"].Value<string>());

                var directive = Assert.Single(Assert.IsType<JArray>(metadata["CloudWatchMetrics"]));
                Assert.Equal("test-namespace", directive["Namespace"].Value<string>());
                Assert.Equal("ServiceName", directive["Dimensions"][0][0].Value<string>());
                var metrics = Assert.IsType<JArray>(directive["Metrics"]);
                Assert.InRange(metrics.Count, 1, Constants.MaxMetricsPerEvent);
                foreach (var metric in metrics)
                {
                    var name = metric["Name"].Value<string>();
                    Assert.Equal("Metric" + serializedMetrics, name);
                    Assert.Equal((double)serializedMetrics, document[name].Value<double>());
                    serializedMetrics++;
                }
            }

            Assert.Equal(metricCount, serializedMetrics);
        }

        [Theory]
        [InlineData("default", "PropertyName")]
        [InlineData("camel", "propertyName")]
        [InlineData("snake", "property_name")]
        public void Serialize_UsesConfiguredNamingStrategyForPropertyValues(string namingStrategy, string expectedName)
        {
            ConfigureNamingStrategy(namingStrategy);
            var context = new MetricsContext();
            context.PutMetric("Count", 1);
            context.PutProperty("CustomProperty", new { PropertyName = "value" });

            var document = JObject.Parse(Assert.Single(context.Serialize()));

            var property = Assert.Single(Assert.IsType<JObject>(document["CustomProperty"]).Properties());
            Assert.Equal(expectedName, property.Name);
            Assert.Equal("value", property.Value.Value<string>());
        }

        public void Dispose()
        {
            JsonConvert.DefaultSettings = _previousSettings;
        }

        private static void ConfigureNamingStrategy(string namingStrategy)
        {
            NamingStrategy strategy = namingStrategy switch
            {
                "camel" => new CamelCaseNamingStrategy(),
                "snake" => new SnakeCaseNamingStrategy(),
                _ => new DefaultNamingStrategy()
            };
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = strategy }
            };
        }
    }
}
