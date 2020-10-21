using System;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Model;

namespace Amazon.CloudWatch.EMF.Environment
{
    public class EC2Environment : AgentBasedEnvironment
    {
        private const string INSTANCE_IDENTITY_URL = "http://169.254.169.254/latest/dynamic/instance-identity/document";
        private const string CFN_EC2_TYPE = "AWS::EC2::Instance";

        private readonly IResourceFetcher _resourceFetcher;
        private EC2Metadata _ec2Metadata;

        public EC2Environment(IConfiguration configuration, IResourceFetcher resourceFetcher) : base(configuration)
        {
            _resourceFetcher = resourceFetcher ?? throw new ArgumentNullException(nameof(resourceFetcher));
        }

        public new bool Probe()
        {
            Uri uri = null;
            try
            {
                uri = new Uri(INSTANCE_IDENTITY_URL);
            }
            catch (Exception ex)
            {
                // log.debug("Failed to construct url: " + INSTANCE_IDENTITY_URL);
                return false;
            }

            try
            {
                _ec2Metadata = _resourceFetcher.Fetch<EC2Metadata>(uri);
                return true;
            }
            catch (EMFClientException ex)
            {
                // log.debug("Failed to get response from: " + endpoint, ex);
            }

            return false;
        }

        public new string Type
        {
            get
            {
                if (!string.IsNullOrEmpty(_configuration.ServiceType))
                {
                    return _configuration.ServiceType;
                }

                if (_ec2Metadata != null)
                {
                    return CFN_EC2_TYPE;
                }

                return Constants.UNKNOWN;
            }
        }

        public new void ConfigureContext(MetricsContext metricsContext)
        {
            if (_ec2Metadata != null)
            {
                metricsContext.PutProperty("imageId", _ec2Metadata.ImageId);
                metricsContext.PutProperty("InstanceId", _ec2Metadata.InstanceId);
                metricsContext.PutProperty("InstanceType", _ec2Metadata.InstanceType);
                metricsContext.PutProperty("PrivateIp", _ec2Metadata.PrivateIp);
                metricsContext.PutProperty("AvailabilityZone", _ec2Metadata.AvailabilityZone);
            }
        }
    }

    // TODO: why is this class static in Java?
    public class EC2Metadata
    {
        internal string ImageId { get; set; }

        internal string AvailabilityZone { get; set; }

        internal string PrivateIp { get; set; }

        internal string InstanceId { get; set; }

        internal string InstanceType { get; set; }
    }
}