using Amazon.CloudWatch.EMF.Sink;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Sink
{
    public class EndpointTests
    {
        [Fact]
        public void TestParseTCPEndpoint()
        {
            string tcpEndpoint = "tcp://10.0.0.1:2580";
            var endpoint = new Endpoint(tcpEndpoint);

            Assert.Equal(endpoint.ToString(), tcpEndpoint);
        }

        [Fact]
        public void TestParseUDPEndpoint()
        {
            string udpEndpoint = "udp://10.0.0.1:2580";
            Endpoint endpoint = new Endpoint(udpEndpoint);

            Assert.Equal(endpoint.ToString(), udpEndpoint);
        }

        [Fact]
        public void TestReturnDefaultEndpointForInvalidURI()
        {
            var defaultEndpoint = Endpoint.DEFAULT_TCP_ENDPOINT;
            var endpoint = new Endpoint("");

            Assert.Equal(defaultEndpoint.Host, endpoint.Host);
            Assert.Equal(defaultEndpoint.Port, endpoint.Port);
            Assert.Equal(defaultEndpoint.CurrentProtocol, endpoint.CurrentProtocol);
        }
    }
}