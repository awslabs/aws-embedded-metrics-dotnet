using System.Collections.Generic;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Model
{
    public class MetricDefinition
    {
        public MetricDefinition(string name, StorageResolution resolution = StorageResolution.STANDARD)
            : this(name, Unit.NONE, new List<double>(), resolution)
        {
        }

        public MetricDefinition(string name, double value, StorageResolution resolution = StorageResolution.STANDARD)
            : this(name, Unit.NONE, new List<double> { value }, resolution)
        {
        }

        public MetricDefinition(string name, Unit unit, double value, StorageResolution resolution = StorageResolution.STANDARD)
            : this(name, unit, new List<double> { value }, resolution)
        {
        }

        public MetricDefinition(string name, Unit unit, List<double> values, StorageResolution storageResolution)
        {
            Name = name;
            Unit = unit;
            Values = values;
            StorageResolution = storageResolution;
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
        public Unit Unit { get; set; }

        [JsonProperty("StorageResolution")]
        public StorageResolution StorageResolution { get; set; }

        // This property is used for serialization decision.
        // Final property serialization will be ignored if set to StandardResolution.
        public bool ShouldSerializeStorageResolution()
        {
            // don't serialize the StorageResolution property if StorageResolution is set to Standard
            return StorageResolution != StorageResolution.STANDARD;
        }
    }
}