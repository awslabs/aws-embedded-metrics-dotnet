using System;
using System.Threading.Tasks;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Amazon.CloudWatch.EMF.Logger
{
    public class MetricsLogger : IMetricsLogger, IDisposable
    {
        public bool FlushPreserveDimensions = true;
        private readonly ILogger _logger;
        private readonly IEnvironment _environment;
        private readonly IEnvironmentProvider _environmentProvider;

        private MetricsContext _context;

        /// <summary>
        /// Creates a Metrics logger (no internal diagnostics)
        /// </summary>
        public MetricsLogger() : this(NullLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Creates a Metrics logger which logs its internal diagnostics to the specified logger.
        /// </summary>
        /// <param name="loggerFactory">the logger where this metrics logger should log its internal diagnostics info.</param>
        public MetricsLogger(ILoggerFactory loggerFactory)
            : this(new EnvironmentProvider(EnvironmentConfigurationProvider.Config, new ResourceFetcher(), loggerFactory), loggerFactory)
        {
        }

        /// <summary>
        /// Creates a Metrics logger.
        /// </summary>
        /// <param name="environmentProvider">provides the environment responsible for annotating events and writing them to a destination</param>
        /// <param name="loggerFactory">the logger where this metrics logger should log its internal diagnostics info.</param>
        public MetricsLogger(IEnvironmentProvider environmentProvider, ILoggerFactory loggerFactory)
            : this(environmentProvider.ResolveEnvironment(), new MetricsContext(), loggerFactory)
        {
        }

        /// <summary>
        /// Creates a Metrics logger.
        /// </summary>
        /// <param name="environment">an environment responsible for annotating events and writing them to a destination</param>
        /// <param name="loggerFactory">the logger where this metrics logger should log its internal diagnostics info.</param>
        public MetricsLogger(IEnvironment environment, ILoggerFactory loggerFactory)
            : this(environment, new MetricsContext(), loggerFactory)
        {
        }

        public MetricsLogger(IEnvironment environment, MetricsContext metricsContext, ILoggerFactory loggerFactory)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (metricsContext == null) throw new ArgumentNullException(nameof(metricsContext));
            _environment = environment;
            _context = metricsContext;
            _logger = loggerFactory.CreateLogger<MetricsLogger>();
            _logger.LogDebug($"Resolved environment {_environment.Name} with sink {_environment.Sink.ToString()}");
        }

        public MetricsLogger(IEnvironmentProvider environmentProvider, MetricsContext metricsContext)
        {
            _context = metricsContext;
            _environment = environmentProvider.ResolveEnvironment();
            _environmentProvider = environmentProvider;
        }

        /// <summary>
        /// Flushes the current context state to the configured sink.
        /// </summary>
        public void Flush()
        {
            _logger.LogDebug($"Configuring context for environment {_environment.Name}");
            ConfigureContextForEnvironment(_context);
            if (_environment.Sink == null)
            {
                var message = $"No Sink is configured for environment `{_environment.GetType().Name}`";
                throw new InvalidOperationException(message);
            }

            _logger.LogDebug("Sending data to sink. {}", _environment.Sink.GetType().Name);

            _environment.Sink.Accept(_context);
            _context = _context.CreateCopyWithContext(this.FlushPreserveDimensions);
        }

        /// <summary>
        /// Sets a property on the published metrics.This is stored in the emitted log data and you are
        /// not charged for this data by CloudWatch Metrics.These values can be values that are useful
        /// for searching on, but have too high cardinality to emit as dimensions to CloudWatch Metrics.
        /// </summary>
        /// <param name="key">the name of the property</param>
        /// <param name="value">the value of the property</param>
        /// <returns>the current logger</returns>
        /// <exception cref="ArgumentException">An invalid parameter is provided.</exception>
        public MetricsLogger PutProperty(string key, object value)
        {
            if ("_aws".Equals(key))
                throw new ArgumentException("'_aws' cannot be used as a property key.");

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
        /// Overwrites all dimensions on this MetricsLogger instance; also overriding default dimensions unless useDefault is set to true
        /// </summary>
        /// <param name="useDefault">whether to use the default dimensions</param>
        /// <param name="dimensionSets">the dimensionSets to set</param>
        /// <returns>the current logger</returns>
        /// <seealso cref="https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/cloudwatch_concepts.html#Dimension"/>
        public MetricsLogger SetDimensions(bool useDefault, params DimensionSet[] dimensionSets)
        {
            _context.SetDimensions(useDefault, dimensionSets);
            return this;
        }

        /// <summary>
        /// Resets all dimensions on this MetricsLogger instance; also resetting default dimensions unless useDefault is set to true
        /// </summary>
        /// <param name="useDefault">whether to use the default dimensions</param>
        /// <returns>the current logger</returns>
        public MetricsLogger ResetDimensions(bool useDefault)
        {
            _context.ResetDimensions(useDefault);
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
        /// <param name="storageResolution">the storage resolution of the metric. Defaults to Standard Storage Resolution</param>
        /// <returns>the current logger</returns>
        public MetricsLogger PutMetric(string key, double value, Unit unit, StorageResolution storageResolution = StorageResolution.STANDARD)
        {
            _context.PutMetric(key, value, unit, storageResolution);
            return this;
        }

        /// <summary>
        /// Puts a metric value without units.
        /// This value will be emitted to CloudWatch Metrics asynchronously and does not contribute to your account TPS limits.
        /// The value will also be available in your CloudWatch Logs.
        /// </summary>
        /// <param name="key">the name of the metric</param>
        /// <param name="value">the value of the metric</param>
        /// <param name="storageResolution">the storage resolution of the metric. Defaults to Standard Storage Resolution</param>
        /// <returns>the current logger</returns>
        public MetricsLogger PutMetric(string key, double value, StorageResolution storageResolution = StorageResolution.STANDARD)
        {
            _context.PutMetric(key, value, Unit.NONE, storageResolution);
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
            Validator.ValidateNamespace(logNamespace);
            _context.Namespace = logNamespace;
            return this;
        }

        /// <summary>
        /// Implement IDisposable so the logger can be used as a scoped dependency in ASP.Net Core DI
        /// </summary>
        public void Dispose()
        {
            this.Flush();
        }

        /// <summary>
        /// Sets the timestamp of the metrics. If not set, current time of the client will be used.
        /// </summary>
        /// <param name="dateTime">Date and Time</param>
        /// <returns>the current logger.</returns>
        public MetricsLogger SetTimestamp(DateTime dateTime)
        {
            Validator.ValidateTimestamp(dateTime);
            _context.SetTimestamp(dateTime);
            return this;
        }

        /// <summary>
        /// Shutdown the associated environment's sink.
        /// </summary>
        public async Task ShutdownAsync()
        {
            await _environment.Sink.Shutdown();
        }

        /// <summary>
        /// Adds default dimensions and properties from the specified environment into the specified metrics context.
        /// </summary>
        /// <param name="context">the context to configure with environment information</param>
        private void ConfigureContextForEnvironment(MetricsContext context)
        {
            if (context.HasDefaultDimensions)
            {
                return;
            }

            var defaultDimensions = new DimensionSet();
            defaultDimensions.AddDimension("ServiceName", _environment.Name);
            defaultDimensions.AddDimension("ServiceType", _environment.Type);
            context.DefaultDimensions = defaultDimensions;
            _environment.ConfigureContext(context);
        }
    }
}
