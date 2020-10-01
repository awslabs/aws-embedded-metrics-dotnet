using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Model
{
    public class MetaData
    {
        internal DateTime Timestamp { get; set; }
        internal List<MetricDirective> CloudWatchMetrics { get; set; }

        private Dictionary<string, Object> CustomFields;

        public MetaData() 
        {
            CloudWatchMetrics = new List<MetricDirective>();
            Timestamp = DateTime.Now;
            CustomFields = new Dictionary<string, Object>();
        }

        internal MetricDirective CreateMetricDirective()
        {
            MetricDirective newMetricDirective = new MetricDirective();
            CloudWatchMetrics.Add(newMetricDirective);
            return newMetricDirective;
        }

        internal bool IsEmpty()
        {
            return !CloudWatchMetrics.Any()
                   || CloudWatchMetrics.TrueForAll(x => !x.HasNoMetrics());
        }

        internal void PutCustomMetadata(string key, Object value)
        {
            CustomFields.Add(key, value);
        }

        internal Dictionary<string, Object> GetCustomMetadata()
        {
            return CustomFields;
        }
    }
}