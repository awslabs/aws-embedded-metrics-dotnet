# aws-embedded-metrics-dotnet

![](https://codebuild.us-west-2.amazonaws.com/badges?uuid=eyJlbmNyeXB0ZWREYXRhIjoiaVZDUUZmc2FNS1c4L3Ntc0I3NzY5cWxzeXhEcTcvRE1CM1VzQmlydWZVTCtBeEszNHA5d0tQRXJ6WThLenpieHJ6SXpPOEJkZWRhTmdGK2dBem1TQTZJPSIsIml2UGFyYW1ldGVyU3BlYyI6InNBVm13VVZ5L1lKa1lRWHUiLCJtYXRlcmlhbFNldFNlcmlhbCI6MX0%3D&branch=main)
[![](https://img.shields.io/nuget/v/Amazon.CloudWatch.EMF)](https://www.nuget.org/packages/Amazon.CloudWatch.EMF/)

Generate CloudWatch Metrics embedded within structured log events. The embedded metrics will be extracted so you can visualize and alarm on them for real-time incident detection. This allows you to monitor aggregated values while preserving the detailed event context that generated them.

## Use Cases

- **Generate custom metrics across compute environments**

  - Easily generate custom metrics from Lambda functions without requiring custom batching code, making blocking network requests or relying on 3rd party software.
  - Other compute environments (EC2, On-prem, ECS, EKS, and other container environments) are supported by installing the [CloudWatch Agent](https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/CloudWatch_Embedded_Metric_Format_Generation_CloudWatch_Agent.html).
    - Examples can be found in [examples/README.md](examples/README.md)

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
    logger.PutMetric("ProcessingLatency", 100, Unit.MILLISECONDS,StorageResolution.STANDARD);
    logger.PutMetric("Memory.HeapUsed", "1600424.0", Unit.BYTES, StorageResolution.HIGH);
    logger.PutProperty("RequestId", "422b1569-16f6-4a03-b8f0-fe3fd9b100f8");
    
}
```

### Graceful Shutdown

In any environment, other than AWS Lambda, we recommend running an out-of-process agent (the CloudWatch Agent or FireLens / Fluent-Bit) to collect the EMF events. 
When using an out-of-process agent, this package will buffer the data asynchronously in process to handle any transient communication issues with the agent.
This means that when the `MetricsLogger` gets flushed, data may not be safely persisted yet.
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
    logger.PutMetric("Memory.HeapUsed", "1600424.0", Unit.BYTES, StorageResolution.HIGH); 
    logger.PutProperty("RequestId", "422b1569-16f6-4a03-b8f0-fe3fd9b100f8");
}

await environment.Sink.Shutdown();
```

### ASP.Net Core

We offer a helper package for ASP.Net Core applications that can be used to simplify the
onboarding process and provide default metrics.

See the example in examples/Amazon.CloudWatch.EMF.Examples.Web to create a logger that is hooked into the 
dependency injection framework and provides default metrics for each request. 
By adding some code to your Startup.cs file, you can get default metrics like the following. 
And of course, you can also emit additional custom metrics from your Controllers.

1. Add the configuration to your Startup file.

```cs
public void ConfigureServices(IServiceCollection services) {
    // Add the necessary services. After this is done, you will have the
    // IMetricsLogger available for dependency injection in your
    // controllers
    services.AddEmf();
}
```

2. Add middleware to add default metrics and metadata to each request.

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
    "Timestamp": 1617649416374,
    "CloudWatchMetrics": [
      {
        "Namespace": "WeatherApp",
        "Metrics": [
          { "Name": "Temperature", "Unit": "None" },
          { "Name": "Time", "Unit": "Milliseconds" }
        ],
        "Dimensions": [
          [ "Controller", "Action" ],
          [ "Controller", "Action", "StatusCode" ]
        ]
      }
    ]
  },
  "TraceId": "|f6eec800-4652f86aef0c7219.",
  "Path": "/WeatherForecast",
  "Controller": "WeatherForecast",
  "Action": "Get",
  "StatusCode": "200",
  "Temperature": -10,
  "Time": 189
}
```

## API

### MetricsLogger

The `MetricsLogger` is the interface you will use to publish embedded metrics. 

- MetricsLogger **PutMetric**(string key, double value)
- MetricsLogger **PutMetric**(string key, double value, Unit unit)
- MetricsLogger **PutMetric**(string key, double value, StorageResolution storageResolution)
- MetricsLogger **PutMetric**(string key, double value, Unit unit, StorageResolution storageResolution)

Adds a new metric to the current logger context. Multiple metrics using the same key will be appended to an array of values. Multiple metrics cannot have the same key but different storage resolutions. Same metric cannot have different storage resolutions otherwise a `InvalidMetricException` will be thrown. The Embedded Metric Format supports a maxumum of 100 metrics per key.

Metrics must meet CloudWatch Metrics requirements, otherwise a `InvalidMetricException` will be thrown. See [MetricDatum](https://docs.aws.amazon.com/AmazonCloudWatch/latest/APIReference/API_MetricDatum.html) for valid values.

##### Storage Resolution.
An OPTIONAL value representing the storage resolution for the corresponding metric. Setting this to `High` specifies this metric as a high-resolution metric, so that CloudWatch stores the metric with sub-minute resolution down to one second. Setting this to `Standard` specifies this metric as a standard-resolution metric, which CloudWatch stores at 1-minute resolution. If a value is not provided, then a default value of `Standard` is assumed. See [Cloud Watch High-Resolution metrics](https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/publishingMetrics.html#high-resolution-metrics)

Example:

```c#
// Standard Resolution Example
logger.PutMetric("ProcessingLatency", 101, Unit.MILLISECONDS);

// High Resolution Example
logger.PutMetric("Memory.HeapUsed", "1600424.0", Unit.BYTES, StorageResolution.HIGH);
```

- MetricsLogger **PutProperty**(string key, object value)

Adds or updates the value for a given property on this context. This value is not submitted to CloudWatch Metrics but is searchable by CloudWatch Logs Insights. This is useful for contextual and potentially high-cardinality data that is not appropriate for CloudWatch Metrics dimensions.

Example:
```c#
logger.PutProperty("AccountId", "123456789");
logger.PutProperty("RequestId", "422b1569-16f6-4a03-b8f0-fe3fd9b100f8");

Dictionary<string, object> payLoad = new Dictionary<string, object>
{
  { "sampleTime", 123456789 },
  { "temperature", 273.0 },
  { "pressure", 101.3 }
};
logger.PutProperty("Payload", payLoad);
```

- MetricsLogger **PutDimensions**(DimensionSet dimensions)

Adds a new set of dimensions that will be associated with all metric values.

**WARNING**: Each dimension set will result in a new CloudWatch metric (even dimension sets with the same values).
If the cardinality of a particular value is expected to be high, you should consider
using `setProperty` instead.

Dimensions must meet CloudWatch Dimensions requirements, otherwise a `InvalidDimensionException` will be thrown. See [Dimensions](https://docs.aws.amazon.com/AmazonCloudWatch/latest/APIReference/API_Dimension.html) for valid values.

Example:

```c#
DimensionSet dimensionSet = new DimensionSet();
dimensionSet.AddDimension("Service", "Aggregator");
dimensionSet.AddDimension("Region", "us-west-2");
logger.PutDimensions(dimensionSet);
```

- MetricsLogger **SetDimensions**(params DimensionSet[] dimensionSets)
- MetricsLogger **SetDimensions**(bool useDefault, params DimensionSet[] dimensionSets)

Explicitly override all dimensions. This will remove the default dimensions unless `useDefault` is set to true.

**WARNING**:Each dimension set will result in a new CloudWatch metric (even dimension sets with the same values).
If the cardinality of a particular value is expected to be high, you should consider
using `setProperty` instead.

Dimensions must meet CloudWatch Dimensions requirements, otherwise a `InvalidDimensionException` will be thrown. See [Dimensions](https://docs.aws.amazon.com/AmazonCloudWatch/latest/APIReference/API_Dimension.html) for valid values.

Examples:
  
```c#
DimensionSet dimensionSet = new DimensionSet();
dimensionSet.AddDimension("Service", "Aggregator");
dimensionSet.AddDimension("Region", "us-west-2");
logger.SetDimensions(true, dimensionSet); // Will preserve default dimensions
```

```c#
DimensionSet dimensionSet = new DimensionSet();
dimensionSet.AddDimension("Service", "Aggregator");
dimensionSet.AddDimension("Region", "us-west-2");
logger.SetDimensions(dimensionSet); // Will remove default dimensions
```

- MetricsLogger **ResetDimensions**(bool useDefault)

Explicitly clear all custom dimensions. Set `useDefault` to `true` to keep using the default dimensions.

- MetricsLogger **SetNamespace**(string logNamespace)

Sets the CloudWatch [namespace](https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/cloudwatch_concepts.html#Namespace) that extracted metrics should be published to. If not set, a default value of aws-embedded-metrics will be used.
Namespaces must meet CloudWatch Namespace requirements, otherwise a `InvalidNamespaceException` will be thrown. See [Namespace](https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/cloudwatch_concepts.html#Namespace) for valid values.

Example:

```c#
logger.SetNamespace("MyApplication")
```

- MetricsLogger **SetTimestamp**(DateTime dateTime)

Sets the timestamp of the metrics. If not set, the current time of the client will be used.
Timestamp must meet CloudWatch requirements, otherwise a `InvalidTimestampException` will be thrown. See [Timestamps](https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/cloudwatch_concepts.html#about_timestamp) for valid values.

Example:
```c#
 logger.SetTimestamp(DateTime.Now);
```


- **Flush**()

Flushes the current MetricsContext to the configured sink and resets all properties and metric values. The namespace and default dimensions will be preserved across flushes. Custom dimensions are preserved by default, but this behavior can be changed by setting `FlushPreserveDimensions = false` on the metrics logger.

Examples:

```c#
flush();  // default dimensions and custom dimensions will be preserved after each flush()
```

```c#
logger.FlushPreserveDimensions = false;
flush();  // only default dimensions will be preserved after each flush()
```

```c#
logger.FlushPreserveDimensions = false;
logger.ResetDimensions(false);  // default dimensions are disabled; no dimensions will be preserved after each flush()
logger.Flush();
```
