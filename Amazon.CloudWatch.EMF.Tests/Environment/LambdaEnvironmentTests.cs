using Amazon.CloudWatch.EMF.Environment;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Environment
{
    public class LambdaEnvironmentTests
    {

        private readonly IFixture _fixture;
        public LambdaEnvironmentTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        }

        //@Test
        //public void testGetNameReturnFunctionsName()
        //{
        //    String expectedName = faker.name().name();
        //    when(SystemWrapper.getenv("AWS_LAMBDA_FUNCTION_NAME")).thenReturn(expectedName);

        //    assertEquals(lambda.getName(), expectedName);
        //}

        //@Test
        //public void testGetTypeReturnCFNLambdaName()
        //{
        //    assertEquals(lambda.getType(), "AWS::Lambda::Function");
        //}

        //@Test
        //public void testGetLogGroupNameReturnFunctionName()
        //{
        //    String expectedName = faker.name().name();
        //    when(SystemWrapper.getenv("AWS_LAMBDA_FUNCTION_NAME")).thenReturn(expectedName);

        //    assertEquals(lambda.getLogGroupName(), expectedName);
        //}

        //@Test
        //public void testConfigureContextAddProperties()
        //{
        //    MetricsContext mc = new MetricsContext();

        //    String expectedEnv = faker.name().name();
        //    when(SystemWrapper.getenv("AWS_EXECUTION_ENV")).thenReturn(expectedEnv);

        //    String expectedVersion = faker.number().digit();
        //    when(SystemWrapper.getenv("AWS_LAMBDA_FUNCTION_VERSION")).thenReturn(expectedVersion);

        //    String expectedLogName = faker.name().name();
        //    when(SystemWrapper.getenv("AWS_LAMBDA_LOG_STREAM_NAME")).thenReturn(expectedLogName);

        //    lambda.configureContext(mc);

        //    assertEquals(mc.getProperty("executionEnvironment"), expectedEnv);
        //    assertEquals(mc.getProperty("functionVersion"), expectedVersion);
        //    assertEquals(mc.getProperty("logStreamId"), expectedLogName);
        //    assertNull(mc.getProperty("traceId"));
        //}

        //@Test
        //public void testContextWithTraceId()
        //{
        //    MetricsContext mc = new MetricsContext();

        //    String expectedTraceId = "Sampled=1;Count=1";
        //    when(SystemWrapper.getenv("_X_AMZN_TRACE_ID")).thenReturn(expectedTraceId);

        //    lambda.configureContext(mc);

        //    assertEquals(mc.getProperty("traceId"), expectedTraceId);
        //}

        //@Test
        //public void testTraceIdWithOhterSampledValue()
        //{
        //    MetricsContext mc = new MetricsContext();

        //    String expectedTraceId = "Sampled=0;Count=1";
        //    when(SystemWrapper.getenv("_X_AMZN_TRACE_ID")).thenReturn(expectedTraceId);

        //    lambda.configureContext(mc);

        //    assertNull(mc.getProperty("traceId"));
        //}

        //@Test
        //public void getCreateSinkReturnsLambdaSink()
        //{
        //    assertTrue(lambda.getSink() instanceof ConsoleSink);
        //}
    }
}