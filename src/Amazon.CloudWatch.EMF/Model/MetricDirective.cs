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
            Namespace = Constants.DefaultNamespace;
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

        internal void PutMetric(string key, double value, StorageResolution storageResolution = StorageResolution.STANDARD)
        {
            PutMetric(key, value, Unit.NONE, storageResolution);
        }

        /// <summary>
        /// Appends the specified metric.
        /// Adds the value an existing metric if one already exists with the specified key or
        /// adds a new metric if one with the specified key does not already exist.
        /// </summary>
        /// <param name="key">the name of the metric</param>
        /// <param name="value">the value of the metric</param>
        /// <param name="unit">the units of the metric</param>
        /// <param name="storageResolution">the storage resolution of the metric. Defaults to Standard Resolution</param>
        internal void PutMetric(string key, double value, Unit unit, StorageResolution storageResolution = StorageResolution.STANDARD)
        {
            var metric = _metrics.FirstOrDefault(m => m.Name == key);
            if (metric != null)
            {
                metric.AddValue(value);
            }
            else
            {
                _metrics.Add(new MetricDefinition(key, value, unit, storageResolution));
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

        internal void PutDimension(DimensionSet dimensionSet)
        {
            // Duplicate dimensions sets are removed before being added to the end of the collection.
            // This ensures the latest dimension key-value is used as a target member on the root EMF node.
            // This operation is O(n^2), but acceptable given sets are capped at 30 dimensions
            List<string> incomingDimensionSetKeys = dimensionSet.DimensionKeys;
            CustomDimensionSets = CustomDimensionSets.Where(existingDimensionSet =>
            {
                if (existingDimensionSet.DimensionKeys.Count != incomingDimensionSetKeys.Count)
                {
                    return true;
                }

                return !existingDimensionSet.DimensionKeys.All(existingDimensionSetKey => incomingDimensionSetKeys.Contains(existingDimensionSetKey));
            }).ToList();

            CustomDimensionSets.Add(dimensionSet);
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

        /// <summary>
        /// Overrides all existing dimensions, optionally suppressing any default dimensions.
        /// </summary>
        /// <param name="useDefault">whether to use the default dimensions</param>
        /// <param name="dimensionSets">the dimension sets to use in lieu of all existing custom and default dimensions</param>
        internal void SetDimensions(bool useDefault, List<DimensionSet> dimensionSets)
        {
            _shouldUseDefaultDimensionSet = useDefault;
            CustomDimensionSets = dimensionSets;
        }

        /// <summary>
        /// Resets all dimensions to the default dimensions.
        /// </summary>
        /// <param name="useDefault">whether to keep the default dimensions</param>
        internal void ResetDimensions(bool useDefault)
        {
            _shouldUseDefaultDimensionSet = useDefault;
            CustomDimensionSets = new List<DimensionSet>();
        }

        internal List<DimensionSet> GetAllDimensionSets()
        {
            if (!_shouldUseDefaultDimensionSet)
            {
                return CustomDimensionSets;
            }

            var dimensions = new DimensionSet();

            dimensions.AddRange(this.DefaultDimensionSet);
            CustomDimensionSets.ForEach(ds => dimensions.AddRange(ds));

            return new List<DimensionSet>() { dimensions };
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
