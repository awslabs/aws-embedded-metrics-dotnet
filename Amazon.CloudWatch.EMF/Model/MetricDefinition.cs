using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Model
{
    public class MetricDefinition
    {
        public MetricDefinition(string name) : this(name, Unit.NONE, new List<double>())
        {
        }

        public MetricDefinition(string name, double value) : this(name, Unit.NONE, new List<double>() { value })
        {
        }

        public MetricDefinition(string name, Unit unit, double value) : this(name, unit, new List<double>() { value })
        {
        }

        public MetricDefinition(string name, Unit unit, List<double> values)
        {
            Name = name;
            Unit = unit;
            Values = values;
        }

        public void AddValue(double value)
        {
            Values.Add(value);
        }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonIgnore]
        public List<double> Values { get; }

        [JsonProperty("Unit")]
        private Unit Unit { get; set; }
    }
}