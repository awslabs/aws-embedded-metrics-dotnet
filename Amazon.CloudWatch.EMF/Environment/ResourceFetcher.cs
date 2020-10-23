using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Amazon.CloudWatch.EMF.Environment
{
    public class ResourceFetcher : IResourceFetcher
    {
        private readonly ILogger _logger;

        public ResourceFetcher() : this(NullLoggerFactory.Instance)
        {
        }

        public ResourceFetcher(ILoggerFactory loggerFactory)
        {
            loggerFactory ??= NullLoggerFactory.Instance;
            _logger = loggerFactory.CreateLogger<ResourceFetcher>();
        }

        /// <summary>
        /// Fetch a json object from a given uri and deserialize it to the specified class: T
        /// </summary>
        /// <returns></returns>
        public T Fetch<T>(Uri endpoint)
        {
            string response = ReadResource(endpoint, "GET");

            return JsonConvert.DeserializeObject<T>(response);
        }

        private string ReadResource(Uri endpoint, string method)
        {
            try
            {
                var httpWebRequest = GetHttpWebRequest(endpoint, method);

                var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    return GetResponse(httpWebResponse);
                }
                else if (httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new EMFClientException("The requested metadata is not found at " + httpWebRequest.RequestUri.AbsolutePath);
                }
                else
                {
                    HandleErrorResponse(httpWebResponse);
                }
            }
            catch (IOException e)
            {
                _logger.LogDebug(
                    "An IOException occurred when connecting to service endpoint: "
                    + endpoint
                    + "\n Retrying to connect "
                    + "again.");
                throw new EMFClientException("Failed to connect to service endpoint: ", e);
            }

            return string.Empty;
        }

        private void HandleErrorResponse(HttpWebResponse httpWebResponse)
        {
            string errorResponse = GetResponse(httpWebResponse);

            try
            {
                /*JsonNode node = Jackson.jsonNodeOf(errorResponse);
                JsonNode code = node.get("code");
                JsonNode message = node.get("message");
                if (code != null && message != null) {
                    errorCode = code.asText();
                    responseMessage = message.asText();
                }

                String exceptionMessage =
                    String.format(
                        "Failed to get resource. Error code: %s, error message: %s ",
                        errorCode, responseMessage);
                throw new EMFClientException(exceptionMessage);*/
            }
            catch (System.Exception exception)
            {
                throw new EMFClientException("Unable to parse error stream: ", exception);
            }
        }

        private HttpWebRequest GetHttpWebRequest(Uri endpoint, string method)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp(endpoint);
            httpWebRequest.Method = method;
            httpWebRequest.Timeout = 1000;
            httpWebRequest.ReadWriteTimeout = 1000;
            return httpWebRequest;
        }

        private string GetResponse(HttpWebResponse response)
        {
            var inputStream = response.GetResponseStream();

            // convert stream to string
            using var reader = new StreamReader(inputStream);
            return reader.ReadToEnd();
        }
    }
}
