using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Amazon.CloudWatch.EMF.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Amazon.CloudWatch.EMF.Sink
{
    public class AgentSink : ISink
    {
        // TODO: set max capacity through config
        private readonly BlockingCollection<string> _queue = new BlockingCollection<string>(10);
        private readonly ILogger _logger;
        private readonly string _logGroupName;
        private readonly string _logStreamName;
        private readonly ISocketClient _socketClient;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _sender;

        public AgentSink(string logGroupName, string logStreamName, Endpoint endpoint, ISocketClientFactory clientFactory)
        : this(logGroupName, logStreamName, endpoint, clientFactory, NullLoggerFactory.Instance)
        {
        }

        public AgentSink(
             string logGroupName,
             string logStreamName,
             Endpoint endpoint,
             ISocketClientFactory clientFactory,
             ILoggerFactory loggerFactory)
        {
            _logGroupName = logGroupName;
            _logStreamName = logStreamName;
            _socketClient = clientFactory.GetClient(endpoint);
            _logger = loggerFactory.CreateLogger<AgentSink>();
            _cancellationTokenSource = new CancellationTokenSource();
            _sender = RunSenderThread(loggerFactory);
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
                    _logger.LogInformation("Enqueuing data.");

                    try
                    {
                        if (!_queue.TryAdd(data))
                        {
                            _logger.LogWarning("Failed to enqueue metrics because the queue was full.");
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        _logger.LogError("Attempted to publish data after the sink has been shutdown. {}", e.Message);
                    }

                    _logger.LogDebug("Data queued successfully.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to serialize the metrics with the exception: ", e);
            }
        }

        public async Task Shutdown()
        {
            _logger.LogDebug("Shutdown requested in AgentSink.");
            _queue.CompleteAdding();
            _cancellationTokenSource.Cancel(false);
            await _sender;
            _socketClient.Dispose();
        }

        private Task RunSenderThread(ILoggerFactory loggerFactory)
        {
            return Task.Run(
                async () =>
                {
                    var logger = loggerFactory.CreateLogger("SenderThread");
                    logger.LogInformation("Starting sender thread.");

                    // TODO: allow force cancellation after timeout
                    while (!_cancellationTokenSource.IsCancellationRequested || _queue.Count > 0)
                    {
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            _logger.LogDebug($"Shutdown request received. {_queue.Count} messages pending.");
                        }

                        var message = _queue.Take();
                        logger.LogDebug("Sending message to socket");

                        // TODO: move into another method to avoid confusion of while loops
                        while (true)
                        {
                            try
                            {
                                await _socketClient.SendMessageAsync(message);
                                logger.LogDebug("Successfully wrote to socket.");
                                break;
                            }
                            catch (Exception e)
                            {
                                logger.LogWarning("Failed to write message to socket. Backing off and trying again. {}", e.Message);
                                Thread.Sleep(1000); // TODO: backoff
                            }
                        }
                    }
                }); // TODO: pass in cancellation token
        }
    }
}