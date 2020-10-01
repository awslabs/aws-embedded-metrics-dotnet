using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.CloudWatch.EMF.Environment;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Amazon.CloudWatch.EMF.Logger
{
    public class MetricsLogger
    {
        private MetricsContext _context;
        private Task<IEnvironment> _environmentFuture;
        private EnvironmentProvider _environmentProvider;

        //TODO: considering switching to ILoggerFactory
        private ILogger _logger;

        /// <summary>
        /// Creates a Metrics logger with no logging
        /// </summary>
        public MetricsLogger()
            : this(NullLogger.Instance)
        {
        }

        public MetricsLogger(ILogger logger)
            : this(new EnvironmentProvider(), logger)
        {

        }
        public MetricsLogger(EnvironmentProvider environmentProvider)
            : this(environmentProvider, new MetricsContext())
        {

        }

        public MetricsLogger(EnvironmentProvider environmentProvider, ILogger logger)
            : this(environmentProvider, new MetricsContext(), logger)
        {
        }

        public MetricsLogger(EnvironmentProvider environmentProvider, MetricsContext metricsContext, ILogger logger)
        {
            _context = metricsContext;
            _environmentFuture = environmentProvider.ResolveEnvironment();
            this._environmentProvider = environmentProvider;
            this._logger = logger;
        }
        
        public MetricsLogger(EnvironmentProvider environmentProvider, MetricsContext metricsContext) {
            _context = metricsContext;
            _environmentFuture = environmentProvider.ResolveEnvironment();
            _environmentProvider = environmentProvider;
        }

        /// <summary>
        /// Flushes the current context state to the configured sink.
        /// </summary>
        // TODO: Support flush asynchronously
        public void Flush()
        {
            IEnvironment environment;
            try
            {
                environment = _environmentFuture.Result;
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation(ex, "Failed to resolve environment. Fallback to default environment: ");
                environment = _environmentProvider.DefaultEnvironment;
            }
            var sink = environment.Sink;
            ConfigureContextForEnvironment(_context, environment);
            sink.Accept(_context);
            _context = _context.CreateCopyWithContext();
        }

        /// <summary>
        /// Sets a property on the published metrics.This is stored in the emitted log data and you are
        /// not charged for this data by CloudWatch Metrics.These values can be values that are useful
        /// for searching on, but have too high cardinality to emit as dimensions to CloudWatch Metrics.
        /// </summary>
        /// <param name="key">the name of the property</param>
        /// <param name="value">the value of the property</param>
        /// <returns>the current logger</returns>
        public MetricsLogger PutProperty(string key, object value)
        {
            _context.PutProperty(key, value);
            return this;
        }

        /// <summary>
        /// Adds a dimension.
        /// This is generally a low cardinality key-value pair that is part of the metric identity.
        /// CloudWatch treats each unique combination of dimensions as a separate metric, even if the metrics have the same metric name
        /// </summary>
        /// <param name="dimensions">the DimensionSet to append</param>
        /// <returns>the current logger</returns>
        /// <seealso cref="*https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/cloudwatch_concepts.html#Dimension"/>
        public MetricsLogger PutDimensions(DimensionSet dimensions)
        {
            _context.PutDimension(dimensions);
            return this;
        }

        /// <summary>
        /// Overwrites all dimensions on this MetricsLogger instance.
        /// </summary>
        /// <param name="dimensionSets">the dimensionSets to set</param>
        /// <returns>the current logger</returns>
        /// <seealso cref="https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/cloudwatch_concepts.html#Dimension"/>
        public MetricsLogger SetDimensions(params DimensionSet[] dimensionSets)
        {
            _context.SetDimensions(dimensionSets);
            return this;
        }

        /// <summary>
        /// Puts a metric value.
        /// This value will be emitted to CloudWatch Metrics asynchronously and does
        /// not contribute to your account TPS limits.The value will also be available in your CloudWatch Logs.
        /// </summary>
        /// <param name="key">the name of the metric</param>
        /// <param name="value">the value of the metric</param>
        /// <param name="unit">the unit of the metric value</param>
        /// <returns>the current logger</returns>
        public MetricsLogger PutMetric(string key, double value, Unit unit)
        {
            _context.PutMetric(key, value, unit);
            return this;
        }

        /// <summary>
        /// Puts a metric value without units.
        /// This value will be emitted to CloudWatch Metrics asynchronously and does
        /// not contribute to your account TPS limits.The value will also be available in your CloudWatch Logs.
        /// </summary>
        /// <param name="key">the name of the metric</param>
        /// <param name="value">the value of the metric</param>
        /// <returns>the current logger</returns>
        public MetricsLogger PutMetric(string key, double value)
        {
            _context.PutMetric(key, value, Unit.NONE);
            return this;
        }

        /// <summary>
        /// Add a custom key-value pair to the Metadata object.
        /// </summary>
        /// <param name="key">the name of the key</param>
        /// <param name="value">the value associated with the key</param>
        /// <returns>the current logger</returns>
        /// <seealso cref="https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/CloudWatch_Embedded_Metric_Format_Specification.html#CloudWatch_Embedded_Metric_Format_Specification_structure_metadata"/>
        public MetricsLogger PutMetadata(string key, object value)
        {
            _context.PutMetadata(key, value);
            return this;
        }

        /// <summary>
        /// Sets the CloudWatch namespace that metrics should be published to.
        /// </summary>
        /// <param name="logNamespace">the namespace of the logs where metrics should be published to.</param>
        /// <returns>the current logger.</returns>
        public MetricsLogger SetNamespace(string logNamespace)
        {
            _context.Namespace = logNamespace;
            return this;
        }

        private void ConfigureContextForEnvironment(MetricsContext context, IEnvironment environment)
        {
            if (context.HasDefaultDimensions)
            {
                return;
            }
            DimensionSet defaultDimensions = new DimensionSet();
            defaultDimensions.AddDimension("LogGroup", environment.LogGroupName);
            defaultDimensions.AddDimension("ServiceName", environment.Name);
            defaultDimensions.AddDimension("ServiceType", environment.Type);
            context.DefaultDimensions = defaultDimensions;
            environment.ConfigureContext(context);
        }
    }
}
