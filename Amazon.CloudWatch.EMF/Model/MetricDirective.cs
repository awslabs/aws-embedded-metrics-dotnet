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

        internal Dictionary<string, MetricDefinition> Metrics { get; set; }

        internal List<DimensionSet> CustomDimensionSets { get; private set; }

       internal DimensionSet DefaultDimensionSet { get; set; }

        private bool ShouldUseDefaultDimension;

        public MetricDirective()
        {
            Namespace = "aws-embedded-metrics";
            Metrics = new Dictionary<string, MetricDefinition>();
            CustomDimensionSets = new List<DimensionSet>();
            DefaultDimensionSet = new DimensionSet();
            ShouldUseDefaultDimension = true;
        }

        void PutDimensionSet(DimensionSet dimensionSet)
        {
            CustomDimensionSets.Add(dimensionSet);
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

        [JsonProperty("Dimensions")]
        internal List<List<string>> AllDimensionKeys
        {
            get
            {
                var keys = new List<List<string>>();
                GetAllDimensionSets().ForEach(s => keys.Add(s.DimensionKeys));
                return keys;
            }
        }

        /// <summary>
        /// Overrides all existing dimensions, including suppressing any default dimensions.
        /// </summary>
        /// <param name="dimensionSets">the dimension sets to use in lieu of all existing custom and default dimensions</param>
        internal void SetDimensions(List<DimensionSet> dimensionSets)
        {
            ShouldUseDefaultDimension = false;
            CustomDimensionSets = dimensionSets;
        }

        internal List<DimensionSet> GetAllDimensionSets()
        {
            if (!ShouldUseDefaultDimension)
            {
                return CustomDimensionSets;
            }
            var dimensions = new List<DimensionSet>() { DefaultDimensionSet };
            dimensions.AddRange(CustomDimensionSets);
            return dimensions;
        }

        public bool HasNoMetrics()
        {
            return !Metrics.Any();
        }
    }
}