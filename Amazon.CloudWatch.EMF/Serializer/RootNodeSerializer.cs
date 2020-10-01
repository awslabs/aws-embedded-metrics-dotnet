using System;
using System.Collections.Generic;
using Amazon.CloudWatch.EMF.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Amazon.CloudWatch.EMF.Serializer
{
    public class RootNodeSerializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var rootNode = value as RootNode;
            writer.WriteStartObject();
            writer.WritePropertyName("_aws");
            
            writer.WritePropertyName("TimeStamp");
            writer.WriteValue(rootNode.AWS.Timestamp);
            
            writer.WritePropertyName("CloudWatchMetrics");
            foreach (MetricDirective metricDirective in rootNode.AWS.CloudWatchMetrics)
            {
                writer.WritePropertyName("NameSpace");
                writer.WriteValue(metricDirective.Namespace);
                
                writer.WritePropertyName("Dimensions");
                AddArrayValues(writer, metricDirective.DefaultDimensions.DimensionKeys);
                
                writer.WritePropertyName("Metrics");
                writer.WriteStartArray();
                foreach (MetricDefinition metricDefinition in metricDirective.Metrics.Values)
                {
                    writer.WriteValue(JsonConvert.SerializeObject(metricDefinition));
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }


        
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(RootNode).IsAssignableFrom(objectType);
        }
        
        private void AddArrayValues(JsonWriter writer, List<string> listString)
        {
            writer.WriteStartArray();
            foreach (string s in listString)
            {
                writer.WriteValue(s);
            }
            writer.WriteEndArray();
        }
    }
}