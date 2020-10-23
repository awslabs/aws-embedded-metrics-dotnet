using System;
using System.Collections.Generic;
using System.Net;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Utils;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Environment
{
    public class ECSEnvironment : AgentBasedEnvironment
    {
        private const string ECS_CONTAINER_METADATA_URI = "ECS_CONTAINER_METADATA_URI";
        private const string FLUENT_HOST = "FLUENT_HOST";
        private const string ENVIRONMENT_TYPE = "AWS::ECS::Container";

        private ECSMetadata _ecsMetadata;
        private IResourceFetcher _resourceFetcher;
        private string _fluentBitEndpoint;
        private string _hostname;

        public ECSEnvironment(IConfiguration configuration, IResourceFetcher resourceFetcher) : base(configuration)
        {
            _resourceFetcher = resourceFetcher ?? throw new ArgumentNullException(nameof(resourceFetcher));
        }

        public override bool Probe()
        {
            string uri = EnvUtils.GetEnv(ECS_CONTAINER_METADATA_URI);

            if (uri == null)
            {
                return false;
            }

            CheckAndSetFluentHost();

            try
            {
                var parsedUri = new Uri(uri);
                _ecsMetadata = _resourceFetcher.Fetch<ECSMetadata>(parsedUri);
                FormatImageName();
                return true;
            }
            catch (Exception)
            {
                // log.debug("Failed to get response from: " + parsedURI, ex);
            }

            return false;
        }

        public override string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(_configuration.ServiceName))
                {
                    return _configuration.ServiceName;
                }

                if (_ecsMetadata != null && !string.IsNullOrEmpty(_ecsMetadata.FormattedImageName))
                {
                    return _ecsMetadata.FormattedImageName;
                }

                return Constants.UNKNOWN;
            }
        }

        public override string Type
        {
            get
            {
                if (!string.IsNullOrEmpty(_configuration.ServiceType))
                {
                    return _configuration.ServiceType;
                }

                return ENVIRONMENT_TYPE;
            }
        }

        public override string LogGroupName
        {
            get
            {
                // FireLens / fluent-bit does not need the log group to be included
                // since configuration of the LogGroup is handled by the fluent bit config file
                if (_fluentBitEndpoint != null)
                {
                    return string.Empty;
                }

                return base.LogGroupName;
            }
        }

        public override void ConfigureContext(MetricsContext metricsContext)
        {
            metricsContext.PutProperty("containerId", GetHostName());
            metricsContext.PutProperty("createdAt", _ecsMetadata.CreatedAt);
            metricsContext.PutProperty("startedAt", _ecsMetadata.StartedAt);
            metricsContext.PutProperty("image", _ecsMetadata.Image);
            metricsContext.PutProperty("cluster", _ecsMetadata.Labels["com.amazonaws.ecs.cluster"]);
            metricsContext.PutProperty("taskArn", _ecsMetadata.Labels["com.amazonaws.ecs.task-arn"]);
        }

        private string GetHostName()
        {
            if (_hostname != null)
            {
                return _hostname;
            }

            try
            {
                _hostname = Dns.GetHostName();
            }
            catch (System.Net.Sockets.SocketException)
            {
                // log.debug("Unable to get hostname: ", ex);
            }

            return _hostname;
        }

        private void CheckAndSetFluentHost()
        {
            string fluentHost = EnvUtils.GetEnv(FLUENT_HOST);
            if (fluentHost != null && string.IsNullOrEmpty(_configuration.AgentEndPoint))
            {
                _fluentBitEndpoint = string.Format("tcp://%s:%d", fluentHost, Constants.DEFAULT_AGENT_PORT);
                _configuration.AgentEndPoint = _fluentBitEndpoint;

                // log.info("Using FluentBit configuration. Endpoint: {}", fluentBitEndpoint);
            }
        }

        private void FormatImageName()
        {
            if (_ecsMetadata != null && _ecsMetadata.Image != null)
            {
                string imageName = _ecsMetadata.Image;
                string[] splitImageNames = imageName.Split("\\/");
                _ecsMetadata.FormattedImageName = splitImageNames[^1];
            }
        }
    }
    
    public class ECSMetadata
    {
        [JsonProperty("name")]
        internal string Name { get; set; }

        [JsonProperty("dockerId")]
        internal string DockerId { get; set; }

        [JsonProperty("dockerName")]
        internal string DockerName { get; set; }

        [JsonProperty("image")]
        internal string Image { get; set; }

        [JsonProperty("formattedImageName")]
        internal string FormattedImageName { get; set; }

        [JsonProperty("imageId")]
        internal string ImageId { get; set; }

        [JsonProperty("labels")]
        internal Dictionary<string, string> Labels { get; set; }

        [JsonProperty("createdAt")]
        internal string CreatedAt { get; set; }

        [JsonProperty("startedAt")]
        internal string StartedAt { get; set; }
    }
}