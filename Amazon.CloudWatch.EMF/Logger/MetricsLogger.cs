using System;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using Amazon.CloudWatch.EMF.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Amazon.CloudWatch.EMF.Logger
{
    public class MetricsLogger : IMetricsLogger, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IEnvironment _environmentFuture;
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
            : this(new EnvironmentProvider(EnvironmentConfigurationProvider.Config, new ResourceFetcher()), loggerFactory)
        {
        }

        public MetricsLogger(IEnvironmentProvider environmentProvider, ILoggerFactory loggerFactory)
            : this(environmentProvider, new MetricsContext(), loggerFactory)
        {
        }

        public MetricsLogger(IEnvironmentProvider environmentProvider, MetricsContext metricsContext, ILoggerFactory loggerFactory)
        {
            if (environmentProvider == null) throw new ArgumentNullException(nameof(environmentProvider));
            if (metricsContext == null) throw new ArgumentNullException(nameof(metricsContext));
            loggerFactory ??= NullLoggerFactory.Instance;

            _context = metricsContext;
            _environmentFuture = environmentProvider.ResolveEnvironment();
            _environmentProvider = environmentProvider;
            _logger = loggerFactory.CreateLogger<MetricsLogger>();
        }

        public MetricsLogger(IEnvironmentProvider environmentProvider, MetricsContext metricsContext)
        {
            _context = metricsContext;
            _environmentFuture = environmentProvider.ResolveEnvironment();
            _environmentProvider = environmentProvider;
        }

        /// <summary>
        /// Flushes the current context state to the configured sink.
        /// </summary>
        public void Flush()
        {
            IEnvironment environment;
            _logger.LogDebug("Resolving the environment");
            try
            {
                environment = _environmentFuture;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Failed to resolve environment. Fallback to default environment.");
                environment = _environmentProvider.DefaultEnvironment;
            }

            _logger.LogDebug($"Resolved environment {environment.Name}");

            // TODO: uncomment this line of code to test serialization results
            // var result = _context.Serialize();

            _logger.LogDebug($"Configuring context for environment  {environment.Name}");
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
            _logger.LogDebug($"Creating new context after flushing logs...");
            _context = _context.CreateCopyWithContext();
            _logger.LogDebug($"New context successfully created.");
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
        /// Implement IDisposable so the logger can be used as a scoped dependency in ASP.Net Core DI
        /// </summary>
        public void Dispose()
        {
            this.Flush();
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

            var defaultDimensions = new DimensionSet();
            defaultDimensions.AddDimension("LogGroup", environment.LogGroupName);
            defaultDimensions.AddDimension("ServiceName", environment.Name);
            defaultDimensions.AddDimension("ServiceType", environment.Type);
            context.DefaultDimensions = defaultDimensions;
            environment.ConfigureContext(context);
        }
    }
}
