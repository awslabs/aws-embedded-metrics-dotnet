using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using Amazon.CloudWatch.EMF.Logger;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.Model;
using NFluent;
using Xunit;

namespace Amazon.CloudWatch.EMF.IntegrationTests
{
    public class MetricsLoggerIntegrationTest
    {
        private IConfiguration _config = EnvironmentConfigurationProvider.Config;
        private static readonly string _serviceName = "IntegrationTests-" + GetLocalHost();
        private static readonly string _serviceType = "AutomatedTest";
        private static readonly string _logGroupName = "aws-emf-java-integ";
        private static readonly string _dimensionName = "Operation";
        private static readonly string _dimensionValue = "Integ-Test-Agent";
        private DimensionSet _dimensions = new DimensionSet(_dimensionName, _dimensionValue);
        private Microsoft.Extensions.Logging.ILogger _logger;

        // TODO: Added constructor for now. @Before used
        public MetricsLoggerIntegrationTest()
        {
            _config.ServiceName = _serviceName;
            _config.ServiceType = _serviceType;
            _config.LogGroupName = _logGroupName;
        }

        [Fact(Timeout = 120_000)]
        public void TestSingleFlushOverTCP()
        {
            var metricName = "TCP-SingleFlush";
            int expectedSamples = 1;
            _config.AgentEndPoint = "tcp://127.0.0.1:25888";

            LogMetric(metricName);

            Check.That(RetryUntilSucceed(BuildRequest(metricName), expectedSamples)).Equals(true);
        }


        [Fact(Timeout = 300_000)]
        public void TestMultipleFlushesOverTCP()
        {
            String metricName = "TCP-MultipleFlushes";
            int expectedSamples = 3;
            _config.AgentEndPoint = "tcp://127.0.0.1:25888";

            LogMetric(metricName);
            LogMetric(metricName);
            Thread.Sleep(500);
            LogMetric(metricName);

            Check.That(RetryUntilSucceed(BuildRequest(metricName), expectedSamples)).Equals(true);
        }

        [Fact(Timeout = 120_000)]
        public void TestSingleFlushOverUDP()
        {
            var metricName = "UDP-SingleFlush";
            int expectedSamples = 1;
            _config.AgentEndPoint = "udp://127.0.0.1:25888";

            LogMetric(metricName);

            Check.That(RetryUntilSucceed(BuildRequest(metricName), expectedSamples)).Equals(true);
        }

        [Fact(Timeout = 300_000)]
        public void TestMultipleFlushOverUDP()
        {
            var metricName = "UDP-MultipleFlush";
            int expectedSamples = 3;
            _config.AgentEndPoint = "udp://127.0.0.1:25888";

            LogMetric(metricName);
            LogMetric(metricName);
            Thread.Sleep(500);
            LogMetric(metricName);

            Check.That(RetryUntilSucceed(BuildRequest(metricName), expectedSamples)).Equals(true);
        }

        private GetMetricStatisticsRequest BuildRequest(String metricName)
        {
            var now = DateTime.Now;
            List<Dimension> dimensions = new List<Dimension>()
            {
                GetDimension("ServiceName", _serviceName),
                GetDimension("ServiceType", _serviceType),
                GetDimension("LogGroup", _logGroupName),
                GetDimension(_dimensionName, _dimensionValue)
            };

            var metricRequest = new GetMetricStatisticsRequest();
            metricRequest.Namespace = "aws-embedded-metrics";
            metricRequest.MetricName = metricName;
            metricRequest.Dimensions = dimensions;
            metricRequest.Period = 60;
            metricRequest.StartTime = now.Subtract(TimeSpan.FromMilliseconds(5000));
            metricRequest.EndTime = now;
            metricRequest.Statistics.Add(Statistic.SampleCount);
            return metricRequest;
        }

        private Dimension GetDimension(string name, string value)
        {
            var res = new Dimension();
            res.Name = name;
            res.Value = value;
            return res;
        }

        private void LogMetric(String metricName)
        {
            MetricsLogger logger = new MetricsLogger(new EnvironmentProvider(EnvironmentConfigurationProvider.Config, new ResourceFetcher()), _logger);
            logger.PutDimensions(_dimensions);
            logger.PutMetric(metricName, 100, Unit.MILLISECONDS);
            logger.Flush();
        }

        private bool RetryUntilSucceed(GetMetricStatisticsRequest request, int expected)
        {
            int attempts = 0;
            while (!CheckMetricExistence(request, expected))
            {
                attempts++;
                Console.Out.Write(
                    "No metrics yet. Sleeping before trying again. Attempt #" + attempts);
                Thread.Sleep(2000);
            }
            return true;
        }

        // TODO: sampleCounts is calculated wrongly
        bool CheckMetricExistence(GetMetricStatisticsRequest request, double expectedSampleCount)
        {
            AmazonCloudWatchClient client = new AmazonCloudWatchClient();
            Task<GetMetricStatisticsResponse> response = client.GetMetricStatisticsAsync(request);

            if (response == null)
            {
                return false;
            }

            var datapoints = response.Result.Datapoints;
            var sampleCounts = 0.0;
            foreach (var datapoint in datapoints)
            {
                sampleCounts += datapoint.SampleCount;
            }
            return sampleCounts.Equals(expectedSampleCount);
        }

        private static string GetLocalHost()
        {
            try
            {
                return Dns.GetHostName();
            }
            catch (System.Exception e)
            {
                return "UnknownHost";
            }
        }
    }
}