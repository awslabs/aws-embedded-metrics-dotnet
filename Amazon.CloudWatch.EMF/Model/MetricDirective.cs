using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Model
{
    /// <summary>
    /// The directives in the Metadata.
    /// This specifies for CloudWatch how to parse and create metrics from the log message.
    /// </summary>
    public class MetricDirective
    {
        [JsonProperty("Namespace")]
        internal string Namespace { get; set; }

        [JsonProperty("Metrics")]
        internal IReadOnlyList<MetricDefinition> Metrics
        {
            get { return _metrics; }
        }

        private List<MetricDefinition> _metrics;

        internal List<DimensionSet> CustomDimensionSets { get; private set; }

        internal DimensionSet DefaultDimensionSet { get; set; }

        private bool _shouldUseDefaultDimensionSet;

        public MetricDirective()
        {
            Namespace = "aws-embedded-metrics";
            _metrics = new List<MetricDefinition>();
            CustomDimensionSets = new List<DimensionSet>();
            DefaultDimensionSet = new DimensionSet();
            _shouldUseDefaultDimensionSet = true;
        }

        public bool HasNoMetrics()
        {
            return !Metrics.Any();
        }

        internal void PutMetric(string key, double value)
        {
            PutMetric(key, value, Unit.NONE);
        }

        /// <summary>
        /// Appends the specified metric.
        /// Adds the value an existing metric if one already exists with the specified key or
        /// adds a new metric if one with the specified key does not already exist.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        internal void PutMetric(string key, double value, Unit unit)
        {
            var metric = _metrics.Where(m => m.Name == key).FirstOrDefault();
            if (metric != null)
            {
                metric.AddValue(value);
            }
            else
            {
                _metrics.Add(new MetricDefinition(key, unit, value));
            }
        }

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
            _shouldUseDefaultDimensionSet = false;
            CustomDimensionSets = dimensionSets;
        }

        internal List<DimensionSet> GetAllDimensionSets()
        {
            if (!_shouldUseDefaultDimensionSet)
            {
                return CustomDimensionSets;
            }

            var dimensions = new List<DimensionSet>() { DefaultDimensionSet };
            dimensions.AddRange(CustomDimensionSets);
            return dimensions;
        }
    }
}