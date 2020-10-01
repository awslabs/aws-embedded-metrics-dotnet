using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Model
{
    public class MetricDirective
    {
        [JsonProperty("Namespace")]
        internal string Namespace { get; set; }

        [JsonIgnore]
        internal Dictionary<String, MetricDefinition> Metrics { get; set; }

        internal List<DimensionSet> Dimensions { get; set; }

        internal DimensionSet DefaultDimensions { get; set; }

        private bool ShouldUseDefaultDimension;

        public MetricDirective()
        {
            Namespace = "aws-embedded-metrics";
            Metrics = new Dictionary<String, MetricDefinition>();
            Dimensions = new List<DimensionSet>();
            DefaultDimensions = new DimensionSet();
            ShouldUseDefaultDimension = true;
        }

        void PutDimensionSet(DimensionSet dimensionSet)
        {
            Dimensions.Add(dimensionSet);
        }

        internal void PutMetric(String key, double value)
        {
            PutMetric(key, value, Unit.NONE);
        }

        internal void PutMetric(String key, double value, Unit unit)
        {
            if (Metrics.ContainsKey(key))
            {
                Metrics.GetValueOrDefault(key).AddValue(value);
            }
            else
            {
                Metrics.Add(key, new MetricDefinition(key, unit, value));
            }
        }
        
        [JsonProperty("Metrics")]
        List<MetricDefinition> AllMetrics => Metrics.Values.ToList();

        List<MetricDefinition> GetAllMetrics()
        {
            return Metrics.Values.ToList();
        }

        [JsonProperty("Dimensions")]
        List<string> AllDimensionKeys
        {
            get
            {
                var keys = new List<string>();
                GetAllDimensions().ForEach(Dim => keys.AddRange(Dim.DimensionKeys));
                return keys;
            }
        }
        List<string> GetAllDimensionKeys()
        {
            var keys = new List<string>();
            GetAllDimensions().ForEach(Dim => keys.AddRange(Dim.DimensionKeys));
            return keys;
        }

        void SetDimensions(List<DimensionSet> dimensionSets)
        {
            ShouldUseDefaultDimension = false;
            Dimensions = dimensionSets;
        }

        List<DimensionSet> GetAllDimensions()
        {
            if (!ShouldUseDefaultDimension)
            {
                return Dimensions;
            }

            var dimensions = new List<DimensionSet>() { DefaultDimensions };
            dimensions.AddRange(Dimensions);
            return dimensions;
        }

        public bool HasNoMetrics()
        {
            return !Metrics.Any();
        }
    }
}