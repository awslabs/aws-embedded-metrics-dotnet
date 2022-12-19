namespace Amazon.CloudWatch.EMF
{
    public class Constants
    {
        public const int MAX_DIMENSION_SET_SIZE = 30;
        public const int MAX_DIMENSION_NAME_LENGTH = 250;
        public const int MAX_DIMENSION_VALUE_LENGTH = 1024;
        public const int MAX_METRIC_NAME_LENGTH = 1024;
        public const int MAX_NAMESPACE_LENGTH = 256;
        public const string VALID_NAMESPACE_REGEX = "^[a-zA-Z0-9._#:/-]+$";

        public const int DEFAULT_AGENT_PORT = 25888;

        public const string UNKNOWN = "Unknown";

        public const int MAX_METRICS_PER_EVENT = 100;

        public const int MAX_DATAPOINTS_PER_METRIC = 100;

        public const string DEFAULT_NAMESPACE = "aws-embedded-metrics";
    }
}