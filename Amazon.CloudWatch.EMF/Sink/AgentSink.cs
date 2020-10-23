using System;
using Amazon.CloudWatch.EMF.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Amazon.CloudWatch.EMF.Sink
{
    public class AgentSink : ISink
    {
        private readonly ILogger _logger;
        private readonly string _logGroupName;
        private readonly string _logStreamName;
        private readonly ISocketClient _socketClient;

        public AgentSink(string logGroupName, string logStreamName, Endpoint endpoint, SocketClientFactory clientFactory) : this(logGroupName, logStreamName, endpoint, clientFactory, NullLoggerFactory.Instance)
        {
        }

        public AgentSink(
             string logGroupName,
             string logStreamName,
             Endpoint endpoint,
             SocketClientFactory clientFactory,
             ILoggerFactory loggerFactory)
        {
            _logGroupName = logGroupName;
            _logStreamName = logStreamName;
            _socketClient = clientFactory.GetClient(endpoint);

            loggerFactory ??= NullLoggerFactory.Instance;

            _logger = loggerFactory.CreateLogger<AgentSink>();
        }

        public void Accept(MetricsContext metricsContext)
        {
            if (!string.IsNullOrEmpty(_logGroupName))
            {
                metricsContext.PutMetadata("LogGroupName", _logGroupName);
            }

            if (!string.IsNullOrEmpty(_logStreamName))
            {
                metricsContext.PutMetadata("LogStreamName", _logStreamName);
            }

            try
            {
                foreach (var data in metricsContext.Serialize())
                {
                    _socketClient.SendMessage(data);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to serialize the metrics with the exception: ", e);
            }
        }
    }
}