using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using Amazon.CloudWatch.EMF.Logger;
using Amazon.CloudWatch.EMF.Model;
using Microsoft.Extensions.Logging;

namespace Amazon.CloudWatch.EMF.Canary
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Canary starting...");

            var init = true;

            var configuration = new Configuration
            {
                LogGroupName = "/Canary/Dotnet/CloudWatchAgent/Metrics",
                EnvironmentOverride = Environments.ECS,
                AgentEndPoint = "tcp://127.0.0.1:25888"
            };

            var loggerFactory = LoggerFactory.Create(builder =>
                        builder
                            .SetMinimumLevel(LogLevel.Information)
                            .AddConsole());

            EnvironmentConfigurationProvider.Config = configuration;

            // get the assembly version (this does not reflect NuGet pre-releases)
            var packageVersion = GetPackageVersion();

            while (true)
            {
                using (var logger = new MetricsLogger(loggerFactory))
                {
                    logger.SetNamespace("Canary");
                    logger.SetTimestamp(DateTime.Now);

                    var dimensionSet = new DimensionSet();
                    dimensionSet.AddDimension("Runtime", "Dotnet");
                    dimensionSet.AddDimension("Platform", "ECS");
                    dimensionSet.AddDimension("Agent", "CloudWatchAgent");
                    dimensionSet.AddDimension("Version", packageVersion);
                    logger.SetDimensions(dimensionSet);

                    using (var currentProcess = System.Diagnostics.Process.GetCurrentProcess())
                    {
                        // https://github.com/dotnet/corefx/blob/3633ea2c6bf9d52029681efeedd84fd7a06eb6ba/src/System.Diagnostics.Process/src/System/Diagnostics/ProcessManager.Linux.cs#L137
                        logger.PutMetric("Memory.RSS", currentProcess.WorkingSet64, Unit.BYTES);
                        logger.PutMetric("Memory.VirtualMemorySize64", currentProcess.VirtualMemorySize64, Unit.BYTES, StorageResolution.HIGH);
                    }

                    logger.PutMetric("Invoke", 1, Unit.NONE);

                    if (init)
                    {
                        init = false;
                        logger.PutMetric("Init", 1, Unit.NONE);
                    }

                    logger.PutMetric("Memory.HeapUsed", GC.GetTotalMemory(false), Unit.BYTES);
                }
                Thread.Sleep(1_000);
            }
        }

        private static String GetPackageVersion()
        {
            foreach (var a in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                if (a.Name.Equals("Amazon.CloudWatch.EMF"))
                {
                    return a.Version?.ToString() ?? "Unknown";
                }
            }

            return "Unknown";
        }
    }
}
