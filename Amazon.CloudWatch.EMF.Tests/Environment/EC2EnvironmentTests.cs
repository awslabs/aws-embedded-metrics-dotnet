using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Environment
{
    public class EC2EnvironmentTests
    {
        private readonly IFixture _fixture;
        public EC2EnvironmentTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        }

        [Fact]
        public void Name_Configuration_Set()
        {
            // Arrange
            var name = "TestService";
            var configuration = _fixture.Create<IConfiguration>();
            var resourceFetcher = _fixture.Create<IResourceFetcher>();
            configuration.ServiceName = name;
            var environment = new EC2Environment(configuration, resourceFetcher);

            // Act
            var environmentName = environment.Name;

            // Assert
            Assert.Equal(name, environmentName);
        }

        [Fact]
        public void Name_Configuration_NotSet()
        {
            // Arrange
            var configuration = _fixture.Create<IConfiguration>();
            var resourceFetcher = _fixture.Create<IResourceFetcher>();
            var environment = new EC2Environment(configuration, resourceFetcher);

            // Act
            var environmentName = environment.Name;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(environmentName));
        }

        [Fact]
        public void Type_Configuration_Set()
        {
            // Arrange
            var type = "TestServiceType";
            var configuration = _fixture.Create<IConfiguration>();
            var resourceFetcher = _fixture.Create<IResourceFetcher>();
            configuration.ServiceType = type;
            var environment = new EC2Environment(configuration, resourceFetcher);

            // Act
            var typeName = environment.Type;

            // Assert
            Assert.Equal(type, typeName);
        }

        [Fact]
        public void Type_Configuration_NotSet()
        {
            // Arrange
            var configuration = _fixture.Create<IConfiguration>();
            var resourceFetcher = _fixture.Create<IResourceFetcher>();
            var environment = new EC2Environment(configuration, resourceFetcher);

            // Act
            var typeName = environment.Type;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(typeName));
        }

        [Fact]
        public void LogStreamName_Configuration_Set()
        {
            // Arrange
            var logStreamName = "TestServiceType";
            var configuration = _fixture.Create<IConfiguration>();
            var resourceFetcher = _fixture.Create<IResourceFetcher>();
            configuration.LogStreamName.Returns(logStreamName);
            var environment = new EC2Environment(configuration, resourceFetcher);

            // Act
            var streamName = environment.LogStreamName;

            // Assert
            Assert.Equal(logStreamName, streamName);
        }

        [Fact]
        public void LogStreamName_Configuration_NotSet()
        {
            // Arrange
            var configuration = _fixture.Create<IConfiguration>();
            var resourceFetcher = _fixture.Create<IResourceFetcher>();
            var environment = new EC2Environment(configuration, resourceFetcher);

            // Act
            var streamName = environment.LogStreamName;

            // Assert
            Assert.Equal(string.Empty, streamName);
        }


        [Fact]
        public void LogGroupName_Configuration_Set()
        {
            // Arrange
            var logGroupName = "TestLogGroup";
            var configuration = _fixture.Create<IConfiguration>();
            var resourceFetcher = _fixture.Create<IResourceFetcher>();
            configuration.LogGroupName = logGroupName;
            var environment = new EC2Environment(configuration, resourceFetcher);

            // Act
            var groupName = environment.LogGroupName;

            // Assert
            Assert.Equal(logGroupName, groupName);
        }

        [Fact]
        public void LogGroupName_Configuration_NotSet()
        {
            // Arrange
            var configuration = _fixture.Create<IConfiguration>();
            var resourceFetcher = _fixture.Create<IResourceFetcher>();
            var environment = new EC2Environment(configuration, resourceFetcher);

            // Act
            var streamName = environment.LogGroupName;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(streamName));
        }

        [Fact]
        public void Probe()
        {
            // Arrange
            var configuration = _fixture.Create<IConfiguration>();
            var resourceFetcher = _fixture.Create<IResourceFetcher>();
            var environment = new EC2Environment(configuration, resourceFetcher);

            // Act
            var result = environment.Probe();

            // Assert
            Assert.True(result);
        }
    }
}