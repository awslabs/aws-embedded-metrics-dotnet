using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Model
{
    public class MetaData
    {
        [JsonProperty("timestamp")]
        private DateTime Timestamp { get; set; }

        [JsonProperty("cloudWatchMetrics")]
        private List<MetricDirective> CloudWatchMetrics { get; set; }

        private Dictionary<string, Object> CustomFields;

        MetaData() {
            CloudWatchMetrics = new List<MetricDirective>();
            Timestamp = DateTime.Now;
            CustomFields = new Dictionary<string, Object>();
        }

        MetricDirective CreateMetricDirective() {
            MetricDirective newMetricDirective = new MetricDirective();
            CloudWatchMetrics.Add(newMetricDirective);
            return newMetricDirective;
        }

        bool IsEmpty()
        {
            return !CloudWatchMetrics.Any()
                   || CloudWatchMetrics.TrueForAll(x => !x.HasNoMetrics());
        }

        void PutCustomMetadata(string key, Object value) {
            CustomFields.Add(key, value);
        }

        Dictionary<string, Object> GetCustomMetadata() {
            return CustomFields;
        }
    }
}