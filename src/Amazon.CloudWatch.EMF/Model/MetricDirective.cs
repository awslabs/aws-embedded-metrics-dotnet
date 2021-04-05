using System.Collections.Generic;
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
        private readonly List<MetricDefinition> _metrics;
        private bool _shouldUseDefaultDimensionSet;

        public MetricDirective()
        {
            Namespace = Constants.DEFAULT_NAMESPACE;
            _metrics = new List<MetricDefinition>();
            CustomDimensionSets = new List<DimensionSet>();
            DefaultDimensionSet = new DimensionSet();
            _shouldUseDefaultDimensionSet = true;
        }

        [JsonProperty("Namespace")]
        internal string Namespace { get; set; }

        [JsonProperty("Metrics")]
        internal List<MetricDefinition> Metrics
        {
            get { return _metrics; }
        }

        internal List<DimensionSet> CustomDimensionSets { get; private set; }

        internal DimensionSet DefaultDimensionSet { get; set; }

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
            var metric = _metrics.FirstOrDefault(m => m.Name == key);
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
                var keys = GetAllDimensionSets()
                    .Where(s => s.DimensionKeys.Any())
                    .Select(s => s.DimensionKeys)
                    .ToList();

                if (keys.Count == 0)
                {
                    keys.Add(new List<string>());
                }

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

            CustomDimensionSets.ForEach(ds => DefaultDimensionSet.AddRange(ds));

            var dimensions = new List<DimensionSet>() { DefaultDimensionSet };
            return dimensions;
        }

        internal MetricDirective DeepCloneWithNewMetrics(List<MetricDefinition> metrics)
        {
            var clone = new MetricDirective();
            clone.CustomDimensionSets = new List<DimensionSet>();
            foreach (var dimension in CustomDimensionSets)
            {
                clone.CustomDimensionSets.Add(dimension.DeepClone());
            }

            clone.DefaultDimensionSet = DefaultDimensionSet.DeepClone();
            clone.Namespace = Namespace;
            clone._shouldUseDefaultDimensionSet = _shouldUseDefaultDimensionSet;
            foreach (var metric in metrics)
            {
                clone._metrics.Add(metric);
            }

            return clone;
        }
    }
}
