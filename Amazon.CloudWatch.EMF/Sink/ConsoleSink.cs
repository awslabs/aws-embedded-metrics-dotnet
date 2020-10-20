using System;
using Amazon.CloudWatch.EMF.Model;

namespace Amazon.CloudWatch.EMF.Sink
{
    /// <summary>
    /// Write log items to the console in JSON format.
    /// </summary>
    public class ConsoleSink : ISink
    {
        public void Accept(MetricsContext context)
        {
            try
            {
                var serializedMetrics = context.Serialize();
                foreach (string metric in serializedMetrics)
                {
                    Console.WriteLine(metric);
                }
            }
            catch (System.Exception e)
            {
                // ToDo: add logging
                // log.error("Failed to serialize a MetricsContext: ", e);
            }
        }
    }
}