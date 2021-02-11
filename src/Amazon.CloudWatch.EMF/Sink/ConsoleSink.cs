using System;
using Amazon.CloudWatch.EMF.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Amazon.CloudWatch.EMF.Sink
{
    /// <summary>
    /// Write log items to the console in JSON format.
    /// </summary>
    public class ConsoleSink : ISink
    {
        private readonly ILogger _logger;

        public ConsoleSink() : this(NullLoggerFactory.Instance)
        {
        }

        public ConsoleSink(ILoggerFactory loggerFactory)
        {
            loggerFactory ??= NullLoggerFactory.Instance;
            _logger = loggerFactory.CreateLogger<ConsoleSink>();
        }

        public void Accept(MetricsContext context)
        {
            try
            {
                var serializedMetrics = context.Serialize();
                foreach (var metric in serializedMetrics)
                {
                    Console.WriteLine(metric);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to serialize a MetricsContext: ", e);
            }
        }
    }
}