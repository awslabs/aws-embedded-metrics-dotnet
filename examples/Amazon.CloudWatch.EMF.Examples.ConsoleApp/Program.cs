using System;
using System.Collections.Generic;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using Amazon.CloudWatch.EMF.Logger;
using Amazon.CloudWatch.EMF.Model;
using Microsoft.Extensions.Logging;

namespace Amazon.CloudWatch.EMF.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a LoggerFactory responsible for internal logging
            var loggerFactory = LoggerFactory.Create(builder => builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddConsole());

            var logger = loggerFactory.CreateLogger("Main");

            // Manually setup the configuration for the library
            var configuration = new Configuration
            {
                ServiceName = "DemoApp",
                ServiceType = "ConsoleApp",
                LogGroupName = "DemoApp",
                EnvironmentOverride = Environments.EC2,
                AgentEndPoint = "tcp://127.0.0.1:25888"
            };

            // create the logger using a DefaultEnvironment which will write over TCP
            var environment = new DefaultEnvironment(configuration, loggerFactory);
            var metrics = new MetricsLogger(environment, loggerFactory);
            for (int i = 0; i < 10; i++)
            {
                EmitMetrics(logger, metrics);
            }

            logger.LogInformation("Shutting down");
            environment.Sink.Shutdown().Wait(TimeSpan.FromSeconds(120));
        }

        private static void EmitMetrics(ILogger logger, IMetricsLogger metrics)
        {

            var dimensionSet = new DimensionSet();
            metrics.SetTimestamp(DateTime.Now);
            dimensionSet.AddDimension("Service", "Aggregator");
            dimensionSet.AddDimension("Region", "us-west-2");
            metrics.SetDimensions(dimensionSet);

            // Standard Resolutions
            metrics.PutMetric("ProcessingLatency", 101, Unit.MILLISECONDS);
            metrics.PutMetric("ProcessingLatency", 100, Unit.MILLISECONDS);
            metrics.PutMetric("ProcessingLatency", 99, Unit.MILLISECONDS);

            // High Resolution
            metrics.PutMetric("Memory.HeapUsed", GC.GetTotalMemory(false), Unit.BYTES, StorageResolution.HIGH);

            metrics.PutMetric("Count", 3, Unit.COUNT);
            metrics.PutProperty("AccountId", "123456789");
            metrics.PutProperty("RequestId", "422b1569-16f6-4a03-b8f0-fe3fd9b100f8");
            metrics.PutProperty("DeviceId", "61270781-c6ac-46f1-baf7-22c808af8162");
            Dictionary<string, object> payLoad = new Dictionary<string, object>
            {
                { "sampleTime", 123456789 },
                { "temperature", 273.0 },
                { "pressure", 101.3 }
            };
            metrics.PutProperty("Payload", payLoad);

            logger.LogInformation("Flushing");
            metrics.Flush();
        }
    }
}
