using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Model
{
    public class MetricDirective
    {
        [JsonProperty("namespace")]
        private string Namespace { get; set; }

        [JsonIgnore]
        private Dictionary<String, MetricDefinition> Metrics { get; set; }

        protected List<DimensionSet> Dimensions{ get; }

        protected DimensionSet DefaultDimensions { get; set; }

        private bool ShouldUseDefaultDimension;

        MetricDirective() {
            Namespace = "aws-embedded-metrics";
            Metrics = new Dictionary<String, MetricDefinition>();
            Dimensions = new List<DimensionSet>();
            DefaultDimensions = new DimensionSet();
            ShouldUseDefaultDimension = true;
        }

        void PutDimensionSet(DimensionSet dimensionSet) {
            Dimensions.Add(dimensionSet);
        }

        void PutMetric(String key, double value) {
            PutMetric(key, value, Unit.NONE);
        }

        void PutMetric(String key, double value, Unit unit) {
            if (Metrics.ContainsKey(key)) {
            }
            else {
            }
        }

        public bool HasNoMetrics()
        {
            return !Metrics.Any();
        }
    }
}