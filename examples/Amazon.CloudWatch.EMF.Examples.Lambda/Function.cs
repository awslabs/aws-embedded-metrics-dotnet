using System.Collections.Generic;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using Amazon.CloudWatch.EMF.Logger;
using Amazon.CloudWatch.EMF.Model;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Amazon.CloudWatch.EMF.Lambda
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string FunctionHandler(string input, ILambdaContext context)
        {
            var envProvider = new EnvironmentProvider(EnvironmentConfigurationProvider.Config, new ResourceFetcher());
            var logger = new MetricsLogger();
            var dimensionSet = new DimensionSet();
            dimensionSet.AddDimension("Service", "Aggregator");
            dimensionSet.AddDimension("Region", "us-west-2");
            logger.PutDimensions(dimensionSet);
            logger.SetNamespace("EMFLambda");
            logger.PutMetric("ProcessingLatency", 101, Unit.MILLISECONDS);
            logger.PutMetric("ProcessingLatency", 100, Unit.MILLISECONDS);
            logger.PutMetric("ProcessingLatency", 99, Unit.MILLISECONDS);
            logger.PutMetric("Count", 3, Unit.COUNT);
            logger.PutProperty("AccountId", "123456789");
            logger.PutProperty("RequestId", "422b1569-16f6-4a03-b8f0-fe3fd9b100f8");
            logger.PutProperty("DeviceId", "61270781-c6ac-46f1-baf7-22c808af8162");
            Dictionary<string, object> payLoad = new Dictionary<string, object>
            {
                { "sampleTime", 123456789 },
                { "temperature", 273.0 },
                { "pressure", 101.3 }
            };
            logger.PutProperty("Payload", payLoad);
            logger.Flush();
            return input?.ToUpper();
        }
    }
}
