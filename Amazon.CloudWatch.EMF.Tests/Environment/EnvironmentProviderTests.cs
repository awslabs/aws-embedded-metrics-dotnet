using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Environment
{
    public class EnvironmentProviderTests
    {
        [Fact]
        public void ResolveEnvironment_ReturnCachedEnv()
        {
            //Arrange
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            var configuration = fixture.Create<IConfiguration>();
            configuration.EnvironmentOverride.Returns(Environments.Local);
            var resourceFetcher = fixture.Create<IResourceFetcher>();
            var environmentProvider = new EnvironmentProvider(configuration, resourceFetcher);

            //Act
            var environment = environmentProvider.ResolveEnvironment();
            var environmentCache = environmentProvider.ResolveEnvironment();

            //Assert
            Assert.Equal(environment, environmentCache);
        }

        [Fact]
        public void ResolveEnvironment_ReturnsLambdaEnvironment()
        {
            //Arrange
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            var configuration = fixture.Create<IConfiguration>();
            configuration.EnvironmentOverride.Returns(Environments.Lambda);
            var resourceFetcher = fixture.Create<IResourceFetcher>();
            var environmentProvider = new EnvironmentProvider(configuration, resourceFetcher);

            //Act
            var environment = environmentProvider.ResolveEnvironment();

            //Assert
            Assert.True(environment is LambdaEnvironment);
        }

        [Fact]
        public void ResolveEnvironment_ReturnsLocalEnvironment()
        {
            //Arrange
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            var configuration = fixture.Create<IConfiguration>();
            configuration.EnvironmentOverride.Returns(Environments.Local);
            var resourceFetcher = fixture.Create<IResourceFetcher>();
            var environmentProvider = new EnvironmentProvider(configuration, resourceFetcher);

            //Act
            var environment = environmentProvider.ResolveEnvironment();

            //Assert
            Assert.True(environment is LocalEnvironment);
        }

        [Fact]
        public void ResolveEnvironment_ReturnsEC2Environment()
        {
            //Arrange
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            var configuration = fixture.Create<IConfiguration>();
            configuration.EnvironmentOverride.Returns(Environments.EC2);
            var resourceFetcher = fixture.Create<IResourceFetcher>();
            var environmentProvider = new EnvironmentProvider(configuration, resourceFetcher);

            //Act
            var environment = environmentProvider.ResolveEnvironment();

            //Assert
            Assert.True(environment is EC2Environment);
        }

        [Fact]
        public void ResolveEnvironment_ReturnsECSEnvironment()
        {
            //Arrange
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            var configuration = fixture.Create<IConfiguration>();
            configuration.EnvironmentOverride.Returns(Environments.ECS);
            var resourceFetcher = fixture.Create<IResourceFetcher>();
            var environmentProvider = new EnvironmentProvider(configuration, resourceFetcher);

            //Act
            var environment = environmentProvider.ResolveEnvironment();

            //Assert
            Assert.True(environment is ECSEnvironment);
        }
    }
}