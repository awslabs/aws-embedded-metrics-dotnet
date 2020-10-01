using System;
using System.Collections.Generic;
using System.Linq;

namespace Amazon.CloudWatch.EMF.Model
{
    public class MetricsContext
    {
        private RootNode _rootNode;

        /// <summary>
        /// holds a reference to _rootNode.MetaData.CloudWatchDirective;
        /// </summary>
        private MetricDirective _metricDirective;

        public MetricsContext() : this(new RootNode())
        {
        }

        public MetricsContext(RootNode rootNode)
        {
            _rootNode = rootNode;
            _metricDirective = rootNode.AWS.CreateMetricDirective();
        }

        public MetricsContext(
            string logNamespace,
            Dictionary<string, object> properties,
            List<DimensionSet> dimensionSets,
            DimensionSet defaultDimensionSet)
        {
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
                    _metricDirective.Dimensions,
                    _metricDirective.DefaultDimensions);
        }

        /**
         * Update the namespace with the parameter.
         *
         * @param namespace The new namespace
         */
        public string Namespace
        {
            get { return _metricDirective.Namespace; }
            set { _metricDirective.Namespace = value; }
        }

        /// <summary>
        /// Gets or Sets the default dimensions for all other dimensions that get added to the context. 
        /// If no custom dimensions are specified, the metrics will be emitted with the defaults.
        /// If custom dimensions are specified, they will be prepended with the default dimensions
        /// </summary>
        public DimensionSet DefaultDimensions
        {
            get { return _metricDirective.DefaultDimensions; }
            set { _metricDirective.DefaultDimensions = value; }
        }

        public bool HasDefaultDimensions
        {
            get { return DefaultDimensions.DimensionKeys.Count > 0; }
        }

        /**
         * Add a metric measurement to the context. Multiple calls using the same key will be stored as
         * an array of scalar values.
         *
         * <pre>{@code
         * metricContext.PutMetric("Latency", 100, Unit.MILLISECONDS)
         * }</pre>
         *
         * @param key Name of the metric
         * @param value Value of the metric
         * @param unit The unit of the metric
         */
        public void PutMetric(String key, double value, Unit unit)
        {
            _metricDirective.PutMetric(key, value, unit);
        }

        /**
         * Add a metric measurement to the context without a unit Multiple calls using the same key will
         * be stored as an array of scalar values.
         *
         * <pre>{@code
         * metricContext.PutMetric("Count", 10)
         * }</pre>
         *
         * @param key Name of the metric
         * @param value Value of the metric
         */
        public void PutMetric(String key, double value)
        {
            PutMetric(key, value, Unit.NONE);
        }

        /**
         * Add a property to this log entry. Properties are additional values that can be associated
         * with metrics. They will not show up in CloudWatch metrics, but they are searchable in
         * CloudWatch Insights.
         *
         * <pre>{@code
         * metricContext.PutProperty("Location", 'US')
         * }</pre>
         *
         * @param name Name of the property
         * @param value Value of the property
         */
        public void PutProperty(String name, Object value)
        {
            _rootNode.PutProperty(name, value);
        }

        public object GetProperty(String name)
        {
            _rootNode.GetProperties().TryGetValue(name, out var value);
            return value;
        }

        /**
         * Add dimensions to the metric context.
         *
         * <pre>{@code
         * metricContext.PutDimension(DimensionSet.of("Dim", "Value" ))
         * }</pre>
         *
         * @param dimensionSet the dimensions set to add
         */
        public void PutDimension(DimensionSet dimensionSet)
        {
            _metricDirective.Dimensions.Add(dimensionSet);
        }

        /**
         * Add a dimension set with single dimension-value entry to the metric context.
         *
         * <pre>{@code
         * metricContext.PutDimension("Dim", "Value" )
         * }</pre>
         *
         * @param dimension the name of the dimension
         * @param value the value associated with the dimension
         */
        public void PutDimension(string dimension, string value)
        {
            _metricDirective.Dimensions[0].AddDimension(dimension, value);
        }

        /** @return the list of dimensions that has been added, including default dimensions. */
        public List<DimensionSet> GetDimensions()
        {
            return _metricDirective.Dimensions;
        }

        /**
         * Update the dimensions. This would override default dimensions
         *
         * @param dimensionSets the dimensionSets to be set
         */
        public void SetDimensions(params DimensionSet[] dimensionSets)
        {
            _metricDirective.Dimensions = dimensionSets.ToList();
        }

        /**
         * Add a key-value pair to the metadata
         *
         * @param key the name of the key
         * @param value the value associated with the key
         */
        public void PutMetadata(String key, Object value)
        {
            _rootNode.AWS.PutCustomMetadata(key, value);
        }

        /**
         * Serialize the metrics in this context to strings. The EMF backend requires no more than 100
         * metrics in one log event. If there're more than 100 metrics, we split the metrics into
         * multiple log events.
         *
         * @return the serialized strings.
         * @throws JsonProcessingException if there's any object that cannot be serialized
         */
        public List<string> Serialize()
        {
            List<RootNode> nodes = new List<RootNode>();
            if (_rootNode.Metrics.Count <= Constants.MAX_METRICS_PER_EVENT)
            {
                nodes.Add(_rootNode);
            }
            else
            {
                //split the root nodes into multiple and serialize each
                int count = 0;
                while (count < _rootNode.Metrics.Count)
                {
                    var metrics = _rootNode.Metrics.Skip(count).Take(Constants.MAX_METRICS_PER_EVENT).ToDictionary(m => m.Key, m => m.Value);
                    var metadata = _rootNode.AWS.CloudWatchMetrics;
                    var node = new RootNode()
                    {
                        Metrics = metrics,
                        //TODO: copy properties and dimensions into rootnode
                    };
                    nodes.Add(node);
                    count += Constants.MAX_METRICS_PER_EVENT;
                }
            }
            var results = new List<string>();
            foreach (var node in nodes)
            {
                results.Add(node.Serialize());
            }
            return results;
        }
    }
}