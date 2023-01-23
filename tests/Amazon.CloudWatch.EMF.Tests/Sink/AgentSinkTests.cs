using System;
using System.Threading.Tasks;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Sink
{
    public class TestClient : ISocketClient
    {
        private string _message;
        public void SendMessage(string message)
        {
            _message = message;
        }

        public string GetMessage()
        {
            return _message;
        }

        public Task SendMessageAsync(string message)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
    public class AgentSinkTests
    {
        private ISocketClientFactory _socketClientFactory;
        private TestClient _client;
        private readonly IFixture _fixture;
        private readonly IConfiguration _config = Config.EnvironmentConfigurationProvider.Config;

        public AgentSinkTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _socketClientFactory = _fixture.Create<ISocketClientFactory>();
            _client = new TestClient();
            _socketClientFactory.GetClient(Endpoint.DEFAULT_TCP_ENDPOINT).Returns(_client);
        }


        [Fact]
        public void TestAccept()
        {
            String prop = "TestProp";
            String propValue = "TestPropValue";
            String logGroupName = "TestLogGroup";
            String logStreamName = "TestLogStream";

            MetricsContext mc = new MetricsContext();

            mc.PutProperty(prop, propValue);
            mc.PutMetric("Time", 10);

            AgentSink sink = new AgentSink(logGroupName, logStreamName, Endpoint.DEFAULT_TCP_ENDPOINT, _socketClientFactory, _config);

            sink.Accept(mc);
        }
    }
}