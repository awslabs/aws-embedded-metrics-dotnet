# aws-embedded-metrics-dotnet

Generate CloudWatch Metrics embedded within structured log events. The embedded metrics will be extracted so you can visualize and alarm on them for real-time incident detection. This allows you to monitor aggregated values while preserving the detailed event context that generated them.

## Use Cases

- **Generate custom metrics across compute environments**

  - Easily generate custom metrics from Lambda functions without requiring custom batching code, making blocking network requests or relying on 3rd party software.
  - Other compute environments (EC2, On-prem, ECS, EKS, and other container environments) are supported by installing the [CloudWatch Agent](https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/CloudWatch_Embedded_Metric_Format_Generation_CloudWatch_Agent.html).

- **Linking metrics to high cardinality context**

  Using the Embedded Metric Format, you will be able to visualize and alarm on custom metrics, but also retain the original, detailed and high-cardinality context which is queryable using [CloudWatch Logs Insights](https://docs.aws.amazon.com/AmazonCloudWatch/latest/logs/AnalyzingLogData.html). For example, the library automatically injects environment metadata such as Lambda Function version, EC2 instance and image ids into the structured log event data.

## Installation

- Using the CLI:

```sh
dotnet add package Amazon.CloudWatch.EMF
```

## Usage

To get a metric logger, you can instantiate it like so.
`MetricsLogger` implements `IDisposable`. 
When the logger is disposed, it will write the metrics to the configured sink.

```c#
using (var logger = new MetricsLogger()) {
    logger.SetNamespace("Canary");
    var dimensionSet = new DimensionSet();
    dimensionSet.AddDimension("Service", "aggregator");
    logger.SetDimensions(dimensionSet);
    logger.PutMetric("ProcessingLatency", 100, Unit.MILLISECONDS);
    logger.PutProperty("RequestId", "422b1569-16f6-4a03-b8f0-fe3fd9b100f8");
}
```

### Graceful Shutdown

In any environment, other than AWS Lambda, we recommend running an out-of-process agent (the CloudWatch Agent or FireLens / Fluent-Bit) to collect the EMF events. 
When using an out-of-process agent, we will buffer the data asynchronously in process to handle any transient communication
issues with the agent. This means that when the `MetricsLogger` gets flushed, data may not be safely persisted yet.
To gracefully shutdown the environment, you can call shutdown on the environment's sink. 
This is an async call that should be awaited. A full example can be found in the examples directory.

```c#
var configuration = new Configuration
{
    ServiceName = "DemoApp",
    ServiceType = "ConsoleApp",
    LogGroupName = "DemoApp",
    EnvironmentOverride = Environments.EC2
};

var environment = new DefaultEnvironment(configuration);

using (var logger = new MetricsLogger()) {
    logger.SetNamespace("Canary");
    var dimensionSet = new DimensionSet();
    dimensionSet.AddDimension("Service", "aggregator");
    logger.SetDimensions(dimensionSet);
    logger.PutMetric("ProcessingLatency", 100, Unit.MILLISECONDS);
    logger.PutProperty("RequestId", "422b1569-16f6-4a03-b8f0-fe3fd9b100f8");
}

await environment.Sink.Shutdown();
```

### ASP.Net Core

We offer a helper package for ASP.Net Core applications that can be used to simplify the
onboarding process and provide default metrics. See the example in examples/Amazon.CloudWatch.EMF.Examples.Web to create a logger that is hooked into the dependency injection framework and provides default metrics for each request. By adding some code to your Startup.cs file, you can get default metrics like the following. And of yourse, you can also emit additional custom metrics from your Controllers.

1. Add the configuration to your Startup file.

```cs
public void ConfigureServices(IServiceCollection services) {
    // Add the necessary services. After this is done, you will have the
    // IMetricsLogger available for dependency injection in your
    // controllers
    services.AddEmf();
}
```

2. Add middleware to add default metrics to each request.

```cs
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Add middleware which will set metric dimensions based on the request routing
    app.UseEmfMiddleware();
}
```

#### Example

```sh
▶ cd examples/Amazon.CloudWatch.EMF.Web
▶ export AWS_EMF_ENVIRONMENT=Local
▶ dotnet run
```

```sh
▶ curl http://localhost:5000
```

```json
{"TraceId":"0HM6EKOBA2CPJ:00000001","Path":"/","StatusCode":"404"}
```

```sh
▶ curl http://localhost:5000/weatherForecast
```

```json
{
  "_aws": {
    "Timestamp": 1613059248188,
    "CloudWatchMetrics": [{
      "Namespace": "aws-embedded-metrics",
      "Metrics": [
        { "Name": "Temperature", "Unit": "None" }
      ],
      "Dimensions": [
        [ "Controller", "Action", "StatusCode" ]
      ]
    }]
  },
  "TraceId": "0HM6EKOBA2CPM:00000001",
  "Path": "/weatherForecast",
  "Controller": "WeatherForecast",
  "Action": "Get",
  "StatusCode": "200",
  "Temperature": -1.0
}
```
