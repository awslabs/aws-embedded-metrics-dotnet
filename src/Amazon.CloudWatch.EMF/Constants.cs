namespace Amazon.CloudWatch.EMF
{
    public class Constants
    {
        public const int MaxDimensionSetSize = 30;
        public const int MaxDimensionNameLength = 250;
        public const int MaxDimensionValueLength = 1024;
        public const int MaxMetricNameLength = 1024;
        public const int MaxNamespaceLength = 256;
        public const string ValidNamespaceRegex = "^[a-zA-Z0-9._#:/-]+$";

        public const int DefaultAgentPort = 25888;

        public const string Unknown = "Unknown";

        public const int MaxMetricsPerEvent = 100;

        public const string DefaultNamespace = "aws-embedded-metrics";
    }
}