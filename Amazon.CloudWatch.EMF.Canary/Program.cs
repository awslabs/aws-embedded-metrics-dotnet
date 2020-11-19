using System;
using System.Diagnostics;
using System.Threading;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Logger;
using Amazon.CloudWatch.EMF.Model;

namespace Amazon.CloudWatch.EMF.Canary
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true) {
                // TODO: get the package version
                var version = "";

                var configuration = new Configuration
                {
                    LogGroupName = "/Canary/Dotnet/CloudWatchAgent/Metrics"
                };
                EnvironmentConfigurationProvider.Config = configuration;

                var logger = new MetricsLogger();
                logger.SetNamespace("Canary");

                var dimensionSet = new DimensionSet();
                dimensionSet.AddDimension("Runtime", "Dotnet");
                dimensionSet.AddDimension("Platform", "ECS");
                dimensionSet.AddDimension("Agent", "CloudWatchAgent");
                dimensionSet.AddDimension("Version", version);
                logger.PutDimensions(dimensionSet);

                Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();

                logger.PutMetric("Invoke", 1, Unit.NONE);
                // There doesn't seem to be a corresponding metric to Memory.HeapTotal for .Net
                // logger.PutMetric("Memory.HeapTotal", GC.GetTotalMemory(false), Unit.BYTES);

                logger.PutMetric("Memory.HeapUsed", GC.GetTotalMemory(false), Unit.BYTES);
                //https://github.com/dotnet/corefx/blob/3633ea2c6bf9d52029681efeedd84fd7a06eb6ba/src/System.Diagnostics.Process/src/System/Diagnostics/ProcessManager.Linux.cs#L137
                logger.PutMetric("Memory.RSS", currentProcess.WorkingSet64, Unit.BYTES);

                logger.Flush();
                Thread.Sleep(30_000);
            }
        }
    }
}
