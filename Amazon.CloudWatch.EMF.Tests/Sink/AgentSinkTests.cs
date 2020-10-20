using System;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Sink
{
    public class AgentSinkTests
    {
        private SocketClientFactory factory;
        private TestClient client;
        private readonly IFixture _fixture;
        
        public AgentSinkTests() {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            var clientFactory = _fixture.Create<SocketClientFactory>();
            client = new TestClient();
            clientFactory.GetClient(Arg.Any<Endpoint>()).Returns(client);
        }

        class TestClient : ISocketClient {

            private string _message;
            public void SendMessage(string message)
            {
                message = _message;
            }
            
            public string GetMessage() {
                return _message;
            }
        }
        
        [Fact]
        public void TestAccept() {
            String prop = "TestProp";
            String propValue = "TestPropValue";
            String logGroupName = "TestLogGroup";
            String logStreamName = "TestLogStream";

            MetricsContext mc = new MetricsContext();

            mc.PutProperty(prop, propValue);
            mc.PutMetric("Time", 10);

            AgentSink sink = new AgentSink(logGroupName, logStreamName, Endpoint.DEFAULT_TCP_ENDPOINT, factory);

            sink.Accept(mc);

        }

    }
}