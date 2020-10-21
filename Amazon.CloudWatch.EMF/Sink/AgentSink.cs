using System;
using Amazon.CloudWatch.EMF.Model;

namespace Amazon.CloudWatch.EMF.Sink
{
     public class AgentSink : ISink
     {
         private string _logGroupName;
         private string _logStreamName;
         private ISocketClient _socketClient;

         public AgentSink(
             string logGroupName,
             string logStreamName,
             Endpoint endpoint,
             SocketClientFactory clientFactory)
         {
             _logGroupName = logGroupName;
             _logStreamName = logStreamName;
             _socketClient = clientFactory.GetClient(endpoint);
         }

         public void Accept(MetricsContext metricsContext)
         {
             if (!String.IsNullOrEmpty(_logGroupName))
             {
                 metricsContext.PutMetadata("LogGroupName", _logGroupName);
             }

             if (!String.IsNullOrEmpty(_logStreamName))
             {
                 metricsContext.PutMetadata("LogStreamName", _logStreamName);
             }

             try
             {
                 foreach (string data in metricsContext.Serialize())
                 {
                     _socketClient.SendMessage(data);
                 }
             }

             // JsonProcessingException
             catch (Exception)
             {
                 // log.error("Failed to serialize the metrics with the exception: ", e);
             }
         }
     }
}