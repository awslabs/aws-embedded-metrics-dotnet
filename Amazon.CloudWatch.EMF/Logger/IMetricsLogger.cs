using Amazon.CloudWatch.EMF.Model;

namespace Amazon.CloudWatch.EMF.Logger
{
    public interface IMetricsLogger
    {
        public void Flush();

        public MetricsLogger PutProperty(string key, object value);

        public MetricsLogger PutDimensions(DimensionSet dimensions);

        public MetricsLogger SetDimensions(params DimensionSet[] dimensionSets);

        public MetricsLogger PutMetric(string key, double value, Unit unit);

        public MetricsLogger PutMetric(string key, double value);

        public MetricsLogger PutMetadata(string key, object value);

        public MetricsLogger SetNamespace(string logNamespace);
    }
}
