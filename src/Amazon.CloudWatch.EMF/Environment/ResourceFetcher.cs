using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
        public T FetchJson<T>(Uri endpoint, string method, Dictionary<string, string> header = null)
        {
            string response = ReadResource(endpoint, method, header).Result;

            return JsonConvert.DeserializeObject<T>(response);
        }

        /// <summary>
        /// Fetch string from a given uri
        /// </summary>
        /// <returns></returns>
        public string FetchString(Uri endpoint, string method, Dictionary<string, string> headers = null)
        {
            string response = ReadResource(endpoint, method, headers).Result;

            return response;
        }

        private async Task<string> ReadResource(Uri endpoint, string method, Dictionary<string, string> headers)
        {
            try
            {
                headers ??= new Dictionary<string, string>();

                var httpResponse = GetResponse(endpoint, method, headers);

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    return await httpResponse.Content.ReadAsStringAsync();
                }

                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new EMFClientException("The requested data is not found at " + endpoint.AbsolutePath);
                }

                throw new EMFClientException("Failed to get resource. Error code: " + httpResponse.StatusCode +
                                             ", error message: " + httpResponse.ReasonPhrase);
            }
            catch (Exception e)
            {
                _logger.LogDebug(
                    "An IOException occurred when connecting to service endpoint: "
                    + endpoint
                    + "\n Attempting to reconnect.");
                throw new EMFClientException("Failed to connect to service endpoint: ", e);
            }
        }

        private HttpResponseMessage GetResponse(Uri endpoint, string method, Dictionary<string, string> headers)
        {
            HttpClient client = new HttpClient();

            var httpMethod = new HttpMethod(method.ToUpper());
            HttpRequestMessage request = new HttpRequestMessage(httpMethod, endpoint);
            foreach (KeyValuePair<string, string> header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            Task<HttpResponseMessage> response = client.SendAsync(request);
            HttpResponseMessage result = response.Result;
            return result;
        }
    }
}
