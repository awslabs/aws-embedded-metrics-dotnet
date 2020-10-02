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
        /// Creates a Metrics logger (no internal diagnostics)
        /// </summary>
        public MetricsLogger()
            : this(NullLogger.Instance)
        {
        }

        /// <summary>
        /// Creates a Metrics logger which logs its internal diagnostics to the specified logger.
        /// </summary>
        /// <param name="logger">the logger where this metrics logger should log its internal diagnostics info.</param>
        public MetricsLogger(ILogger logger)
            : this(new EnvironmentProvider(), logger)
        {
        }


        public MetricsLogger(EnvironmentProvider environmentProvider, ILogger logger)
            : this(environmentProvider, new MetricsContext(), logger)
        {
        }

        public MetricsLogger(EnvironmentProvider environmentProvider, MetricsContext metricsContext, ILogger logger)
        {
            if (environmentProvider == null) throw new ArgumentNullException(nameof(environmentProvider));
            if (metricsContext == null) throw new ArgumentNullException(nameof(metricsContext));
            if (logger == null) logger = NullLogger.Instance;

            _context = metricsContext;
            _environmentFuture = environmentProvider.ResolveEnvironment();
            this._environmentProvider = environmentProvider;
            this._logger = logger;
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
                _logger.LogInformation(ex, "Failed to resolve environment. Fallback to default environment.");
                environment = _environmentProvider.DefaultEnvironment;
            }
            //TODO: uncomment this line of code to test serialization results
            var result = _context.Serialize();
            ConfigureContextForEnvironment(_context, environment);
            var sink = environment.Sink;
            if (sink == null)
            {
                var message = $"No Sink is configured for environment `{environment.GetType().Name}`";
                var ex = new InvalidOperationException(message);
                _logger.LogError(ex, message);
                throw ex;
            }
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
        /// Overwrites all dimensions on this MetricsLogger instance; also overriding default dimensions
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
        /// This value will be emitted to CloudWatch Metrics asynchronously and does not contribute to your account TPS limits.
        /// The value will also be available in your CloudWatch Logs.
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

        /// <summary>
        /// Adds default dimensions and properties from the specified environment into the specified metrics context.
        /// </summary>
        /// <param name="context">the context to configure with environment information</param>
        /// <param name="environment">the environment to read dimensions and properties from</param>
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
