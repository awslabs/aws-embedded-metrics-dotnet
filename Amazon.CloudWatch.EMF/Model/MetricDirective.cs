using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        protected List<DimensionSet> Dimensions{ get; set; }

        protected DimensionSet DefaultDimensions { get; set; }

        private bool ShouldUseDefaultDimension;

        public MetricDirective() {
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
            if (Metrics.ContainsKey(key))
            {
                Metrics.GetValueOrDefault(key).AddValue(value);
            }
            else
            {
                Metrics.Add(key, new MetricDefinition(key, unit, value));
            }
        }

        // TODO: add [JsonProperty("metrics")], Originally Collection is returned in java code
        List<MetricDefinition> GetAllMetrics()
        {
            return Metrics.Values.ToList();
        }

        List<HashSet<String>> GetAllDimensionKeys()
        {
            var keys = new List<HashSet<String>>();
            GetAllDimensions().ForEach(Dim => keys.Add(Dim.GetDimensionKeys()));
            return keys;
        }

        void SetDimensions(List<DimensionSet> dimensionSets) {
            ShouldUseDefaultDimension = false;
            Dimensions = dimensionSets;
        }

        List<DimensionSet> GetAllDimensions() {
            if (!ShouldUseDefaultDimension) {
                return Dimensions;
            }

            if (!Dimensions.Any()) {
                return new List<DimensionSet>(){DefaultDimensions};
            }

            Dimensions.ForEach(dim => DefaultDimensions.AddDimension(dim));
            return Dimensions;
        }

        public bool HasNoMetrics()
        {
            return !Metrics.Any();
        }
    }
}