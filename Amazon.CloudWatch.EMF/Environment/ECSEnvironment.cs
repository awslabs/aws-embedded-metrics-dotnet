using System;
using System.Collections.Generic;
using System.Net;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Utils;

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

        public new bool Probe()
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

        public new string Name
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

        public new string Type
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

        public new string LogGroupName
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

        public void ConfigureContext(MetricsContext metricsContext)
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

    // TODO: why is this class static in Java?
    public class ECSMetadata
    {
        internal string Name { get; set; }

        internal string DockerId { get; set; }

        internal string DockerName { get; set; }

        internal string Image { get; set; }

        internal string FormattedImageName { get; set; }

        internal string ImageId { get; set; }

        internal Dictionary<string, string> Labels { get; set; }

        internal string CreatedAt { get; set; }

        internal string StartedAt { get; set; }
    }
}