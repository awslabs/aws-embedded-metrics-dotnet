using System;
using System.Collections.Generic;
using System.Linq;

namespace Amazon.CloudWatch.EMF.Model
{
    public class MetricsContext
    {
        private readonly RootNode _rootNode;

        /// <summary>
        /// holds a reference to _rootNode.MetaData.CloudWatchDirective;
        /// </summary>
        private readonly MetricDirective _metricDirective;

        public MetricsContext() : this(new RootNode())
        {
        }

        public MetricsContext(RootNode rootNode)
        {
            if (rootNode == null) throw new ArgumentNullException(nameof(rootNode));
            _rootNode = rootNode;
            _metricDirective = rootNode.AWS.MetricDirective;
        }

        public MetricsContext(
            string logNamespace,
            Dictionary<string, object> properties,
            List<DimensionSet> dimensionSets,
            DimensionSet defaultDimensionSet) : this()
        {
            if (string.IsNullOrEmpty(logNamespace)) throw new ArgumentNullException(nameof(logNamespace));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            if (dimensionSets == null) throw new ArgumentNullException(nameof(dimensionSets));
            if (defaultDimensionSet == null) throw new ArgumentNullException(nameof(defaultDimensionSet));

            Namespace = logNamespace;
            DefaultDimensions = defaultDimensionSet;
            foreach (DimensionSet dimension in dimensionSets)
            {
                PutDimension(dimension);
            }

            foreach (var property in properties)
            {
                PutProperty(property.Key, property.Value);
            }
        }

        /// <summary>
        /// Creates a new MetricsContext with the same namespace, properties,
        /// and dimensions as this one but empty metrics-directive collection.
        /// </summary>
        /// <returns></returns>
        public MetricsContext CreateCopyWithContext()
        {
            return new MetricsContext(
                    _metricDirective.Namespace,
                    _rootNode.GetProperties(),
                    _metricDirective.CustomDimensionSets,
                    _metricDirective.DefaultDimensionSet);
        }

        /// <summary>
        /// Gets or sets the namespace for all metrics in this context.
        /// </summary>
        public string Namespace
        {
            get { return _metricDirective.Namespace; }
            set { _metricDirective.Namespace = value; }
        }

        /// <summary>
        /// Gets or Sets the default dimensions for the context.
        /// If no custom dimensions are specified, the metrics will be emitted with these defaults.
        /// If custom dimensions are specified, they will be prepended with these default dimensions
        /// </summary>
        public DimensionSet DefaultDimensions
        {
            get { return _metricDirective.DefaultDimensionSet; }
            set { _metricDirective.DefaultDimensionSet = value; }
        }

        /// <summary>
        /// Indicates whether default dimensions have already been set on this context.
        /// </summary>
        public bool HasDefaultDimensions
        {
            get { return DefaultDimensions.DimensionKeys.Count > 0; }
        }

        /// <summary>
        /// Add a metric measurement to the context.
        /// Multiple calls using the same key will be stored as an array of scalar values.
        /// </summary>
        /// <example>
        /// metricContext.PutMetric("Latency", 100, Unit.MILLISECONDS)
        /// </example>
        /// <param name="key">the name of the metric</param>
        /// <param name="value">the value of the metric</param>
        /// <param name="unit">the units of the metric</param>
        public void PutMetric(string key, double value, Unit unit)
        {
            _metricDirective.PutMetric(key, value, unit);
        }

        /// <summary>
        /// Add a metric measurement to the context without a unit.
        /// Multiple calls using the same key will be stored as an array of scalar values.
        /// </summary>
        /// <example>
        /// metricContext.PutMetric("Count", 10)
        /// </example>
        /// <param name="key">the name of the metric</param>
        /// <param name="value">the value of the metric</param>
        public void PutMetric(string key, double value)
        {
            PutMetric(key, value, Unit.NONE);
        }

        /// <summary>
        /// Add a property to this log entry.
        /// Properties are additional values that can be associated with metrics.
        /// They will not show up in CloudWatch metrics, but they are searchable in CloudWatch Insights.
        /// </summary>
        /// <example>
        /// metricContext.PutProperty("Location", 'US')
        /// </example>
        /// <param name="name">the name of the property</param>
        /// <param name="value">the value of the property</param>
        public void PutProperty(string name, object value)
        {
            _rootNode.PutProperty(name, value);
        }

        /// <summary>
        /// Gets the value of the property with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>the value of the property with the specified name, or <c>null</c> if no property with that name has been set</returns>
        public object GetProperty(string name)
        {
            _rootNode.GetProperties().TryGetValue(name, out var value);
            return value;
        }

        /// <summary>
        /// Add dimensions to the metric context.
        /// </summary>
        /// <param name="dimensionSet">the dimensions set to add.</param>
        public void PutDimension(DimensionSet dimensionSet)
        {
            _metricDirective.CustomDimensionSets.Add(dimensionSet);
        }

        /// <summary>
        /// Adds a dimension set with single dimension-value entry to the metric context.
        /// </summary>
        /// <example>
        /// metricContext.PutDimension("ExampleDimension", "ExampleValue")
        /// </example>
        /// <param name="dimension">the name of the new dimension.</param>
        /// <param name="value">the value of the new dimension.</param>
        public void PutDimension(string dimension, string value)
        {
            var dimensionSet = new DimensionSet();
            dimensionSet.AddDimension(dimension, value);
            _metricDirective.CustomDimensionSets.Add(dimensionSet);
        }

        /// <summary>
        /// Gets all dimension sets that have been added, including default dimensions.
        /// </summary>
        /// <returns>the list of dimensions that has been added, including default dimensions.</returns>
        public List<DimensionSet> GetAllDimensionSets()
        {
            return _metricDirective.GetAllDimensionSets();
        }

        /// <summary>
        /// Update the dimensions to the specified list; also overriding default dimensions
        /// </summary>
        /// <param name="dimensionSets">the dimensionSets to use instead of all existing dimensions and default dimensions.</param>
        public void SetDimensions(params DimensionSet[] dimensionSets)
        {
            _metricDirective.SetDimensions(dimensionSets.ToList());
        }

        /// <summary>
        /// Adds the specified key-value pair to the metadata.
        /// </summary>
        /// <param name="key">the name of the key.</param>
        /// <param name="value">the value to associate with the specified key.</param>
        public void PutMetadata(string key, object value)
        {
            _rootNode.AWS.CustomMetadata.Add(key, value);
        }

        /// <summary>
        /// Serializes the metrics in this context to strings.
        /// The EMF backend requires no more than 100 metrics in one log event.
        /// If there are more than 100 metrics, we split the metrics into multiple log events (strings).
        /// </summary>
        /// <returns>the serialized strings</returns>
        public List<string> Serialize()
        {
            var nodes = new List<RootNode>();

            if (_rootNode.AWS.MetricDirective.Metrics.Count <= Constants.MAX_METRICS_PER_EVENT && NoMetricWithTooManyDataPoints(_rootNode))
            {
                nodes.Add(_rootNode);
            }
            else
            {
                Dictionary<String, MetricDefinition> metrics = new Dictionary<string, MetricDefinition>();
                Queue<MetricDefinition> metricDefinitions =
                    new Queue<MetricDefinition>(_rootNode.AWS.MetricDirective.Metrics);
                while (metricDefinitions.Count > 0)
                {
                    MetricDefinition metric = metricDefinitions.Dequeue();

                    if (metrics.Count == Constants.MAX_METRICS_PER_EVENT || metrics.ContainsKey(metric.Name))
                    {
                        var node = _rootNode.DeepCloneWithNewMetrics(metrics.Values.ToList());
                        nodes.Add(node);
                        metrics = new Dictionary<string, MetricDefinition>();
                    }

                    if (metric.Values.Count <= Constants.MAX_DATAPOINTS_PER_METRIC)
                    {
                        metrics.Add(metric.Name, metric);
                    }
                    else
                    {
                        metrics.Add(
                            metric.Name,
                            new MetricDefinition(metric.Name, metric.Unit, metric.Values.Take(Constants.MAX_DATAPOINTS_PER_METRIC).ToList()));
                        metricDefinitions.Enqueue(
                            new MetricDefinition(
                                metric.Name,
                                metric.Unit,
                                metric.Values.Skip(Constants.MAX_DATAPOINTS_PER_METRIC).Take(metric.Values.Count).ToList()));
                    }
                }

                if (metrics.Count > 0)
                {
                    var node = _rootNode.DeepCloneWithNewMetrics(metrics.Values.ToList());
                    nodes.Add(node);
                }
            }

            var results = new List<string>();
            foreach (var node in nodes)
            {
                results.Add(node.Serialize());
            }

            return results;
        }

        private bool NoMetricWithTooManyDataPoints(RootNode node)
        {
            return node.AWS.MetricDirective.Metrics.All(metric => metric.Values.Count <= Constants.MAX_DATAPOINTS_PER_METRIC);
        }
    }
}