using System.Collections.Generic;
using Amazon.CloudWatch.EMF.Model;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Model
{
    public class RootNode
    {
        private MetaData aws;
        private readonly Dictionary<string, object> _properties;
        public RootNode()
        {
            _properties = new Dictionary<string, object>();
        }

        public void PutProperty(string key, object value)
        {
            _properties.Add(key, value);
        }

        /// <summary>
        /// Return the target members that are referenced by metrics, dimensions and properties.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetTargetMembers()
        {
            var targetMembers = new Dictionary<string, object>();
            CopyAll(_properties, targetMembers);
            CopyAll(GetDimensions(), targetMembers);
            foreach (MetricDirective metricDirective in aws.CloudWatchMetrics)
            {
                foreach (MetricDefinition metricDefinition in metricDirective.Metrics.Values)
                {
                    List<double> values = metricDefinition.Values;
                    targetMembers.Add(metricDefinition.Name, values.Count == 1 ? (object) values[0] : values);
                }
            }
            return targetMembers;
        }

        
        /// <summary>
        /// Return a list of all dimensions that are referenced by each dimension set.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetDimensions()
        {
            var dimensions = new Dictionary<string, string>();
            foreach (MetricDirective metricDirective in aws.CloudWatchMetrics)
            {
                foreach (DimensionSet dimensionSet in metricDirective.Dimensions)
                {
                    //TODO
                    //CopyAll(dimensionSet.g);
                }
            }
                return dimensions;
        }
        
        private Dictionary<string, MetricDefinition> Metrics()
        {
            return aws.CloudWatchMetrics[0].Metrics;
        }

        private void CopyAll(Dictionary<string, object> sourceDictionary, Dictionary<string, object> targetDictionary)
        {
            foreach (KeyValuePair<string, object> kvp in sourceDictionary)
            {
                targetDictionary.Add(kvp.Key, kvp.Value);
            }
        }
        private void CopyAll(Dictionary<string, string> sourceDictionary, Dictionary<string, object> targetDictionary)
        {
            foreach (KeyValuePair<string, string> kvp in sourceDictionary)
            {
                targetDictionary.Add(kvp.Key, kvp.Value);
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(aws);
        }
    }
}