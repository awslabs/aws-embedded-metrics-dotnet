using System;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using Amazon.CloudWatch.EMF.Sink;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Environment
{
    public class LocalEnvironmentTests
    {
        private readonly IFixture _fixture;
        public LocalEnvironmentTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        }

        [Fact]
        public void Probe_Returns_False()
        {
            var configuration = _fixture.Create<IConfiguration>();
            var ctor = new LocalEnvironment(configuration);

            Assert.False(ctor.Probe());
        }

        [Fact]
        public void Name_Configuration_Set()
        {
            // Arrange
            var configuration = _fixture.Create<IConfiguration>();
            var environment = new LocalEnvironment(configuration);

            // Act
            var environmentName = environment.Name;

            // Assert
            Assert.Equal("Unknown", environmentName);
        }


        [Fact]
        public void Type_Configuration_Set()
        {
            // Arrange
            var configuration = _fixture.Create<IConfiguration>();
            var environment = new LocalEnvironment(configuration);

            // Act
            var typeName = environment.Type;

            // Assert
            Assert.Equal("Unknown", typeName);
        }

        [Fact]
        public void Sink_Return()
        {
            // Arrange
            var configuration = _fixture.Create<IConfiguration>();
            var environment = new LocalEnvironment(configuration);

            // Act
            var sink = environment.Sink;

            // Assert
            Assert.True(sink is ConsoleSink);
        }
    }
}