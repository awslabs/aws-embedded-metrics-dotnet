using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Model
{
    public class MetricDefinition
    {
        [JsonProperty("name")]
        private string Name { get; set; }

        [JsonProperty("unit")]
        private Unit Unit { get; set; }

        [JsonIgnore]
        private List<Double> Values { get; set; }

        public MetricDefinition(string name) : this(name, Unit.NONE,new List<double>()) { }

        public MetricDefinition(string name, double value) : this(name, Unit.NONE,new List<double>(){value}) { }

        public MetricDefinition(string name, Unit unit, double value) : this(name, unit, new List<double>(){value}) { }

        public MetricDefinition(String name, Unit unit, List<Double> values)
        {
            Name = name;
            Unit = unit;
            Values = values;
        }

        public void AddValue(double value) {
            Values.Add(value);
        }
    }
}