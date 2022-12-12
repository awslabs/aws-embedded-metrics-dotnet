using System;
using System.Collections.Generic;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Environment
{
    public class EC2Environment : AgentBasedEnvironment
    {
        // Documentation for configuring instance metadata can be found here:
        // https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/configuring-instance-metadata-service.html
        private const string INSTANCE_IDENTITY_URL = "http://169.254.169.254/latest/dynamic/instance-identity/document";
        private const string TOKEN_URL = "http://169.254.169.254/latest/api/token";
        private const string TOKEN_REQUEST_HEADER_KEY = "X-aws-ec2-metadata-token-ttl-seconds";
        private const string TOKEN_REQUEST_HEADER_VALUE = "21600";
        private const string METADATA_REQUEST_HEADER_KEY = "X-aws-ec2-metadata-token";
        private const string CFN_EC2_TYPE = "AWS::EC2::Instance";

        private readonly ILogger _logger;
        private readonly IResourceFetcher _resourceFetcher;
        private string _token;
        private EC2Metadata _ec2Metadata;

        public EC2Environment(IConfiguration configuration, IResourceFetcher resourceFetcher)
        : this(configuration, resourceFetcher, NullLoggerFactory.Instance)
        {
        }

        public EC2Environment(IConfiguration configuration, IResourceFetcher resourceFetcher, ILoggerFactory loggerFactory)
        : base(configuration, loggerFactory)
        {
            _resourceFetcher = resourceFetcher ?? throw new ArgumentNullException(nameof(resourceFetcher));

            loggerFactory ??= NullLoggerFactory.Instance;
            _logger = loggerFactory.CreateLogger<EC2Environment>();
        }

        public override bool Probe()
        {
            Uri tokenUri = null;
            var tokenRequestHeader = new Dictionary<string, string>();
            tokenRequestHeader.Add(TOKEN_REQUEST_HEADER_KEY, TOKEN_REQUEST_HEADER_VALUE);
            try
            {
                tokenUri = new Uri(TOKEN_URL);
            }
            catch (Exception)
            {
                _logger.LogDebug("Failed to construct url: " + TOKEN_URL);
                return false;
            }

            try
            {
                _token = _resourceFetcher.FetchString(tokenUri, "PUT", tokenRequestHeader);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Failed to get response from: " + tokenUri, ex);
                return false;
            }

            Uri metadataUri = null;
            var metadataRequestHeader = new Dictionary<string, string>();
            metadataRequestHeader.Add(METADATA_REQUEST_HEADER_KEY, _token);
            try
            {
                metadataUri = new Uri(INSTANCE_IDENTITY_URL);
            }
            catch (Exception)
            {
                _logger.LogDebug("Failed to construct url: " + INSTANCE_IDENTITY_URL);
                return false;
            }

            try
            {
                _ec2Metadata = _resourceFetcher.FetchJson<EC2Metadata>(metadataUri, "GET", metadataRequestHeader);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Failed to get response from: " + metadataUri, ex);
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