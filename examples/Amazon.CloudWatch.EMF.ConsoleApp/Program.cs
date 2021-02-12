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
            var loggerFactory = LoggerFactory.Create(builder => builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddConsole());
            
            var logger = loggerFactory.CreateLogger("Main");
            
            var configuration = new Configuration("Test Console App", "Console", "TestConsoleApp", "TestConsoleApp", "",
                Environments.EC2);
            EnvironmentConfigurationProvider.Config = configuration;

            var environmentProvider = new EnvironmentProvider(EnvironmentConfigurationProvider.Config, new ResourceFetcher(), loggerFactory);
            var metrics = new MetricsLogger(environmentProvider, loggerFactory);

            var dimensionSet = new DimensionSet();
            dimensionSet.AddDimension("Service", "Aggregator");
            dimensionSet.AddDimension("Region", "us-west-2");
            metrics.PutDimensions(dimensionSet);
            metrics.PutMetric("ProcessingLatency", 101, Unit.MILLISECONDS);
            metrics.PutMetric("ProcessingLatency", 100, Unit.MILLISECONDS);
            metrics.PutMetric("ProcessingLatency", 99, Unit.MILLISECONDS);
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

            logger.LogInformation("Shutting down");
            metrics.ShutdownAsync().Wait(TimeSpan.FromSeconds(30));
        }
    }
}
