using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Model
{
    /// <summary>
    /// Represents the CloudWatch Metrics Metadata appended to the CloudWatch log 
    /// and used by CloudWatch to parse metrics out of the log's properties.
    /// </summary>
    public class MetaData
    {
        //TODO: should be epic format (not timestamp)
        [JsonProperty]
        internal DateTime Timestamp { get; set; }

        /// <summary>
        /// Represents the MetricDirective.
        /// NOTE: serialization emits an Array, but only a single MetricDirective is supported by this library.
        /// </summary>
        [JsonProperty]
        internal IReadOnlyCollection<MetricDirective> CloudWatchMetrics { get; set; }

        internal MetricDirective MetricDirective { get { return CloudWatchMetrics.First(); } }

        internal Dictionary<string, object> CustomMetadata { get; set; } = new Dictionary<string, object>();

        public MetaData()
        {
            CloudWatchMetrics = new List<MetricDirective>() { new MetricDirective() };
            Timestamp = DateTime.Now;
            CustomMetadata = new Dictionary<string, object>();
        }

        internal bool IsEmpty()
        {
            return !MetricDirective.HasNoMetrics();
        }
    }
}