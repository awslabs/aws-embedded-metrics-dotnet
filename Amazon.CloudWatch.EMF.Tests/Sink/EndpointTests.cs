using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Serializer;
using Amazon.CloudWatch.EMF.Sink;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Newtonsoft.Json;
using NFluent;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Sink
{
    public class EndpointTests
    {
        [Fact]
        public void TestParseTCPEndpoint() {
            string tcpEndpoint = "tcp://173.9.0.12:2580";
            var endpoint = Endpoint.FromURL(tcpEndpoint);

            Assert.Equal(endpoint.ToString(), tcpEndpoint);
        }

        [Fact]
        public void TestParseUDPEndpoint() {
            string udpEndpoint = "udp://173.9.0.12:2580";
            string tcpEndpoint = "tcp://173.9.0.12:2580";
            Endpoint endpoint = Endpoint.FromURL(udpEndpoint);

            Assert.Equal(endpoint.ToString(), tcpEndpoint);
        }

        [Fact]
        public void TestReturnDefaultEndpointForInvalidURI() {
            string unsupportedEndpoint = "http://173.9.0.12:2580";
            Endpoint endpoint = Endpoint.FromURL(unsupportedEndpoint);
            Endpoint endpointFromEmptyString = Endpoint.FromURL("");

            Assert.Equal(endpointFromEmptyString, Endpoint.DEFAULT_TCP_ENDPOINT);
        }
    }
}