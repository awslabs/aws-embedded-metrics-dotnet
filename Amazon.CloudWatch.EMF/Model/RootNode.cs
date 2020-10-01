using System;
using System.Collections.Generic;
using System.Linq;
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

    //[JsonConverter(typeof(RootNodeSerializer))]
    public class RootNode
    {
        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

        [JsonProperty("_aws")]
        internal MetaData AWS { get; } = new MetaData();

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
        [JsonExtensionData]
        private Dictionary<string, object> LogMessageProperties
        {
            get
            {
                var targetMembers = new Dictionary<string, object>();
                foreach (var property in _properties)
                {
                    targetMembers.Add(property.Key, property.Value);
                }
                foreach (var dimension in GetDimensions())
                {
                    targetMembers.Add(dimension.Key, dimension.Value);
                }
                foreach (MetricDirective metricDirective in AWS.CloudWatchMetrics)
                {
                    foreach (MetricDefinition metricDefinition in metricDirective.Metrics.Values)
                    {
                        List<double> values = metricDefinition.Values;
                        targetMembers.Add(metricDefinition.Name, values.Count == 1 ? (object)values[0] : values);
                    }
                }
                //TODO: include metadata
                return targetMembers;
            }
        }


        /// <summary>
        /// Return a list of all dimensions that are referenced by each dimension set.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetDimensions()
        {
            var dimensions = new Dictionary<string, string>();
            foreach (MetricDirective metricDirective in AWS.CloudWatchMetrics)
            {
                foreach (DimensionSet dimensionSet in metricDirective.CustomDimensionSets)
                {
                    //TODO: fix
                    //dimensionSet.get
                }
            }
            return dimensions;
        }

        internal Dictionary<string, MetricDefinition> Metrics
        {
            get { return AWS.CloudWatchMetrics[0].Metrics; }
            set { AWS.CloudWatchMetrics[0].Metrics = value; }
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