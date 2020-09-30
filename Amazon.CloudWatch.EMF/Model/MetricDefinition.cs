using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Model
{
    public class MetricDefinition
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("unit")]
        private Unit Unit { get; set; }

        [JsonIgnore]
        public List<Double> Values { get; set; }

        MetricDefinition(string name) : this(name, Unit.NONE,new List<double>()) { }

        MetricDefinition(string name, double value) : this(name, Unit.NONE,new List<double>(){value}) { }

        MetricDefinition(string name, Unit unit, double value) : this(name, unit, new List<double>(){value}) { }

        MetricDefinition(String name, Unit unit, List<Double> values)
        {
            Name = name;
            Unit = unit;
            Values = values;
        }

        void AddValue(double value) {
            Values.Add(value);
        }
    }
}