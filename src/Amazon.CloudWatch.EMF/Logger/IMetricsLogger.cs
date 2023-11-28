using System;
using System.Threading.Tasks;
using Amazon.CloudWatch.EMF.Model;

namespace Amazon.CloudWatch.EMF.Logger
{
    public interface IMetricsLogger
    {
        public void Flush();

        public Task ShutdownAsync();

        public MetricsLogger PutProperty(string key, object value);

        public MetricsLogger PutDimensions(DimensionSet dimensions);

        public MetricsLogger SetDimensions(params DimensionSet[] dimensionSets);

        public MetricsLogger PutMetric(string key, double value, Unit unit, StorageResolution storageResolution = StorageResolution.STANDARD);

        public MetricsLogger PutMetric(string key, double value, StorageResolution storageResolution = StorageResolution.STANDARD);

        public MetricsLogger PutMetadata(string key, object value);

        public MetricsLogger SetNamespace(string logNamespace);

        public MetricsLogger SetTimestamp(DateTime dateTime);
    }
}
