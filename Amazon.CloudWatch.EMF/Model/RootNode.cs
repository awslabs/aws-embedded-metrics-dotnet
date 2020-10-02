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
        /// Emits the target members that are referenced by metrics, dimensions and properties.
        /// </summary>
        /// <returns></returns>
        [JsonExtensionData]
        private Dictionary<string, object> CloudWatchLogMembers
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
                foreach(var data in AWS.CustomMetadata)
                {
                    targetMembers.Add(data.Key, data.Value);
                }
                foreach (MetricDefinition metricDefinition in AWS.MetricDirective.Metrics)
                {
                    List<double> values = metricDefinition.Values;
                    targetMembers.Add(metricDefinition.Name, values.Count == 1 ? (object)values[0] : values);
                }
                return targetMembers;
            }
        }


        /// <summary>
        /// Return a list of all dimensions from all dimension sets.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetDimensions()
        {
            var dimensions = new Dictionary<string, string>();
            var dimensionSets = AWS.MetricDirective.GetAllDimensionSets();
            foreach (DimensionSet dimensionSet in dimensionSets)
            {
                foreach (var dimension in dimensionSet.Dimensions)
                {
                    dimensions.Add(dimension.Key, dimension.Value);
                }
            }
            return dimensions;
        }
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}