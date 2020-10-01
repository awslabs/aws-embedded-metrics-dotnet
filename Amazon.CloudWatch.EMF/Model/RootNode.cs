using System;
using System.Collections.Generic;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Serializer;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Model
{
    /*
    EXAMPLE OF DESIRED SERILIZATION OUTPUT:

    {
        "_aws": {
            "Timestamp": 1559748430481
            "CloudWatchMetrics": [ 
                {
                    "Namespace": "lambda-function-metrics",
                    "Dimensions": [ [ "functionVersion" ] ],
                    "Metrics": [
                        {
                            "Name": "Time",
                            "Unit": "Milliseconds"
                        }
                    ]
                }
            ]
        },
        "time": 100,
     }     
     */

    [JsonConverter(typeof(RootNodeSerializer))]
    public class RootNode
    {
        private MetaData _aws;
        
        private readonly Dictionary<string, object> _properties;
        public RootNode()
        {
            _properties = new Dictionary<string, object>();
        }

        internal MetaData AWS => _aws;

        public void PutProperty(string key, object value)
        {
            _properties.Add(key, value);
        }

        public Dictionary<string, object> GetProperties()
        {
            return _properties;
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
            foreach (MetricDirective metricDirective in _aws.CloudWatchMetrics)
            {
                foreach (MetricDefinition metricDefinition in metricDirective.Metrics.Values)
                {
                    List<double> values = metricDefinition.Values;
                    targetMembers.Add(metricDefinition.Name, values.Count == 1 ? (object)values[0] : values);
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
            foreach (MetricDirective metricDirective in _aws.CloudWatchMetrics)
            {
                foreach (DimensionSet dimensionSet in metricDirective.Dimensions)
                {
                    //TODO
                    //CopyAll(dimensionSet.g);
                }
            }
            return dimensions;
        }

        internal Dictionary<string, MetricDefinition> Metrics
        {
            get { return _aws.CloudWatchMetrics[0].Metrics; }
            set { _aws.CloudWatchMetrics[0].Metrics = value; }
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
            return JsonConvert.SerializeObject(this);
        }
    }
}