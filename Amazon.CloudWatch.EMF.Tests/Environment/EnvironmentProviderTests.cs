using System.Collections.Generic;
using System.Linq.Expressions;
using Amazon.CloudWatch.EMF.Environment;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Environment
{
    public class EnvironmentProviderTests
    {

        private readonly IFixture _fixture;

        public EnvironmentProviderTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        }

        [Fact]
        public async void ResolveEnvironment_ReturnCachedEnv()
        {
            //Arrange
            var environmentProvider = new EnvironmentProvider();

            //Act
            var environment = await environmentProvider.ResolveEnvironment();
            var environmentCache = await environmentProvider.ResolveEnvironment();

            //Assert
            Assert.Equal(environment, environmentCache);
        }

        //[Fact]
        //public async void ResolveEnvironment_ReturnsLambdaEnvironment()
        //{
        //    //Arrange
        //    var lambdaEnvironment = _fixture.Create<LambdaEnvironment>();
        //    lambdaEnvironment.Probe().Returns(true);
        //    var envronments = new IEnvironment[] { lambdaEnvironment };
        //    var environmentProvider = new EnvironmentProvider(envronments);

        //    //Act
        //    var environment = await environmentProvider.ResolveEnvironment();

        //    //Assert
        //    Assert.True(environment is LambdaEnvironment);
        //}

        //    @Test
        //    public void testResolveEnvironmentReturnsLambdaFromOverride()
        //{
        //    PowerMockito.mockStatic(EnvironmentConfigurationProvider.class);
        //when(EnvironmentConfigurationProvider.getConfig()).thenReturn(config);
        //when(config.getEnvironmentOverride()).thenReturn(Environments.Lambda);

        //environmentProvider.cleanResolvedEnvironment();

        //CompletableFuture<Environment> resolvedEnvironment =
        //        environmentProvider.resolveEnvironment();

        //assertTrue(resolvedEnvironment.join() instanceof LambdaEnvironment);
        //    }

        //    @Test
        //    public void testResolveEnvironmentReturnsDefaultEnvironment()
        //{
        //    PowerMockito.mockStatic(EnvironmentConfigurationProvider.class);
        //when(EnvironmentConfigurationProvider.getConfig()).thenReturn(config);
        //when(config.getEnvironmentOverride()).thenReturn(Environments.Agent);

        //environmentProvider.cleanResolvedEnvironment();

        //CompletableFuture<Environment> resolvedEnvironment =
        //        environmentProvider.resolveEnvironment();

        //assertTrue(resolvedEnvironment.join() instanceof DefaultEnvironment);
        //    }

        //    @Test
        //    public void testResolveEnvironmentReturnsEC2Environment()
        //{
        //    PowerMockito.mockStatic(EnvironmentConfigurationProvider.class);
        //when(EnvironmentConfigurationProvider.getConfig()).thenReturn(config);
        //when(config.getEnvironmentOverride()).thenReturn(Environments.EC2);

        //environmentProvider.cleanResolvedEnvironment();

        //CompletableFuture<Environment> resolvedEnvironment =
        //        environmentProvider.resolveEnvironment();

        //assertTrue(resolvedEnvironment.join() instanceof EC2Environment);
        //    }

        //    @Test
        //    public void testResolveEnvironmentReturnsECSEnvironment()
        //{
        //    PowerMockito.mockStatic(EnvironmentConfigurationProvider.class);
        //when(EnvironmentConfigurationProvider.getConfig()).thenReturn(config);
        //when(config.getEnvironmentOverride()).thenReturn(Environments.ECS);

        //environmentProvider.cleanResolvedEnvironment();

        //CompletableFuture<Environment> resolvedEnvironment =
        //        environmentProvider.resolveEnvironment();

        //assertTrue(resolvedEnvironment.join() instanceof ECSEnvironment);
        //    }

        //    @Test
        //    public void testResolveEnvironmentReturnsLocalEnvironment()
        //{
        //    PowerMockito.mockStatic(EnvironmentConfigurationProvider.class);
        //when(EnvironmentConfigurationProvider.getConfig()).thenReturn(config);
        //when(config.getEnvironmentOverride()).thenReturn(Environments.Local);

        //environmentProvider.cleanResolvedEnvironment();

        //CompletableFuture<Environment> resolvedEnvironment =
        //        environmentProvider.resolveEnvironment();

        //assertTrue(resolvedEnvironment.join() instanceof LocalEnvironment);
        //    }
    }
}