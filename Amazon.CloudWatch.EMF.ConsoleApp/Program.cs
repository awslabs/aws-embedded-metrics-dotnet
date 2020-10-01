using System;
using System.Collections.Generic;
using Amazon.CloudWatch.EMF;
using Amazon.CloudWatch.EMF.Model;

namespace Amazon.CloudWatch.EMF.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new Amazon.CloudWatch.EMF.Logger.MetricsLogger();
            //logger.PutDimensions(new Model.DimensionSet("Service", "Aggregator"));
            logger.PutMetric("ProcessingLatency", 100, Unit.MILLISECONDS);
            logger.PutProperty("AccountId", "123456789");
            logger.PutProperty("RequestId", "422b1569-16f6-4a03-b8f0-fe3fd9b100f8");
            logger.PutProperty("DeviceId", "61270781-c6ac-46f1-baf7-22c808af8162");
            Dictionary<string, object> payLoad = new Dictionary<string, object>();
            payLoad.Add("sampleTime", 123456789);
            payLoad.Add("temperature", 273.0);
            payLoad.Add("pressure", 101.3);
            logger.PutProperty("Payload", payLoad);
            logger.Flush();
            Console.ReadLine();
        }
    }
}
