using System;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Model;
using Newtonsoft.Json;

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

        public override bool Probe()
        {
            Uri uri = null;
            try
            {
                uri = new Uri(INSTANCE_IDENTITY_URL);
            }
            catch (Exception)
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

        public override string Type
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

        public override void ConfigureContext(MetricsContext metricsContext)
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

    public class EC2Metadata
    {
        [JsonProperty("imageId")]
        internal string ImageId { get; set; }

        [JsonProperty("availabilityZone")]
        internal string AvailabilityZone { get; set; }

        [JsonProperty("privateIp")]
        internal string PrivateIp { get; set; }

        [JsonProperty("instanceId")]
        internal string InstanceId { get; set; }

        [JsonProperty("instanceType")]
        internal string InstanceType { get; set; }
    }
}