using System;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;
using Amazon.CloudWatch.EMF.Utils;
using Microsoft.Extensions.Logging;

namespace Amazon.CloudWatch.EMF.Environment
{
    public class LambdaEnvironment : IEnvironment
    {
        private const string AWS_EXECUTION_ENV = "AWS_EXECUTION_ENV";
        private const string LAMBDA_FUNCTION_NAME = "AWS_LAMBDA_FUNCTION_NAME";
        private const string LAMBDA_FUNCTION_VERSION = "AWS_LAMBDA_FUNCTION_VERSION";
        private const string LAMBDA_LOG_STREAM = "AWS_LAMBDA_LOG_STREAM_NAME";
        private const string TRACE_ID = "_X_AMZN_TRACE_ID";
        private const string LAMBDA_CFN_NAME = "AWS::Lambda::Function";

        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        private ISink _sink = null;

        public LambdaEnvironment(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public bool Probe()
        {
            string functionName = EnvUtils.GetEnv(LAMBDA_FUNCTION_NAME);
            return functionName != null;
        }

        public string Name
        {
            get
            {
                string functionName = EnvUtils.GetEnv(LAMBDA_FUNCTION_NAME);
                return functionName != null ? functionName : "Unknown";
            }
        }

        public string Type => LAMBDA_CFN_NAME;

        public string LogGroupName => Name;

        public void ConfigureContext(MetricsContext context)
        {
            AddProperty(context, "executionEnvironment", EnvUtils.GetEnv(AWS_EXECUTION_ENV));
            AddProperty(context, "functionVersion", EnvUtils.GetEnv(LAMBDA_FUNCTION_VERSION));

            var traceId = GetSampledTrace();
            if (!string.IsNullOrEmpty(traceId))
            {
                AddProperty(context, "traceId", traceId);
            }
        }

        public ISink Sink
        {
            get
            {
                return _sink ??= new ConsoleSink(_loggerFactory);
            }
        }

        private void AddProperty(MetricsContext context, string key, string value)
        {
            if (value != null)
            {
                context.PutProperty(key, value);
            }
        }

        private string GetSampledTrace()
        {
            string traceId = EnvUtils.GetEnv(TRACE_ID);
            if (traceId != null && traceId.Contains("Sampled=1", StringComparison.OrdinalIgnoreCase))
            {
                return traceId;
            }

            return string.Empty;
        }
    }
}