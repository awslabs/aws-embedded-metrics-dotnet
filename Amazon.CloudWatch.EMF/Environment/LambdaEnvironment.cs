namespace Amazon.CloudWatch.EMF.Environment
{
    using System;
    using Amazon.CloudWatch.EMF.Model;
    using Amazon.CloudWatch.EMF.Sink;
    public class LambdaEnvironment : IEnvironment
    {
        private const string AWS_EXECUTION_ENV = "AWS_EXECUTION_ENV";
        private const string LAMBDA_FUNCTION_NAME = "AWS_LAMBDA_FUNCTION_NAME";
        private const string LAMBDA_FUNCTION_VERSION = "AWS_LAMBDA_FUNCTION_VERSION";
        private const string LAMBDA_LOG_STREAM = "AWS_LAMBDA_LOG_STREAM_NAME";
        private const string TRACE_ID = "_X_AMZN_TRACE_ID";
        private const string LAMBDA_CFN_NAME = "AWS::Lambda::Function";

        private ISink _sink = null;
        
        // TODO: support probing asynchronously
        public bool Probe()
        {
            string functionName = GetEnv(LAMBDA_FUNCTION_NAME);
            return functionName != null;
        }

        public string Name
        {
            get
            {
                string functionName = GetEnv(LAMBDA_FUNCTION_NAME);
                return functionName != null ? functionName : "Unknown";
            }
        }

        public string Type => LAMBDA_CFN_NAME;

        public string LogGroupName => Name;

        public void ConfigureContext(MetricsContext context) 
        {
            AddProperty(context, "executionEnvironment", GetEnv(AWS_EXECUTION_ENV));
            AddProperty(context, "functionVersion", GetEnv(LAMBDA_FUNCTION_VERSION));
            AddProperty(context, "logStreamId", GetEnv(LAMBDA_LOG_STREAM));

            var traceId = GetSampledTrace();
            if(!string.IsNullOrEmpty(traceId))
                AddProperty(context, "traceId", traceId);
        }

        public ISink Sink
        {
            get
            {
                return _sink ??= new ConsoleSink();
            }
        }

        private void AddProperty(MetricsContext context, string key, string value) 
        {
            if (value != null) 
            {
                //context.putProperty(key, value);
            }
        }
        
        //TODO: Java uses optional string, validate if this works as expected
        private string GetSampledTrace()
        {
            string traceId = GetEnv(TRACE_ID);
            if (traceId != null && traceId.Contains("Sampled=1"))
            {
                return traceId;
            }
            return string.Empty;
        }

        //TODO: Java one calls SystemWrapper, revisit if thats needed for mocking
        private string GetEnv(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }
    }
}