using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon.CloudWatch.EMF.Logger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Amazon.CloudWatch.EMF.Web
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseEmfMiddleware(this IApplicationBuilder app)
        {
            app.UseEmfMiddleware((context, logger) => {
                var dimensions = new Model.DimensionSet();
                var dimensionsWithStatusCode = new Model.DimensionSet();

                var endpoint = context.GetEndpoint();
                if (endpoint != null) {
                    var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

                    dimensions.AddDimension("Controller", actionDescriptor.ControllerName);
                    dimensions.AddDimension("Action", actionDescriptor.ActionName);

                    dimensionsWithStatusCode.AddDimension("Controller", actionDescriptor.ControllerName);
                    dimensionsWithStatusCode.AddDimension("Action", actionDescriptor.ActionName);
                }

                dimensionsWithStatusCode.AddDimension("StatusCode", context.Response.StatusCode.ToString());
                logger.SetDimensions(dimensions, dimensionsWithStatusCode);

                // Include the X-Ray trace id if it is set
                // https://docs.aws.amazon.com/xray/latest/devguide/xray-concepts.html#xray-concepts-tracingheader
                var xRayTraceId = context.Request.Headers["X-Amzn-Trace-Id"];
                if (!String.IsNullOrEmpty(xRayTraceId) && xRayTraceId.Count > 0) {
                    logger.PutProperty("XRayTraceId", xRayTraceId[0]);
                }

                // If the request contains a w3c trace id, let's embed it in the logs
                // Otherwise we'll include the TraceIdentifier which is the connectionId:requestCount
                // identifier.
                // https://www.w3.org/TR/trace-context/#traceparent-header
                logger.PutProperty("TraceId", Activity.Current?.Id ?? context?.TraceIdentifier);

                if (!String.IsNullOrEmpty(Activity.Current?.TraceStateString)) {
                    logger.PutProperty("TraceState", Activity.Current.TraceStateString);
                }

                logger.PutProperty("Path", context.Request.Path);
                return Task.CompletedTask;
            });
        }

        public static void UseEmfMiddleware(this IApplicationBuilder app, Func<HttpContext, IMetricsLogger, Task> action)
        {
            app.Use(async (context, next) =>
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                await next.Invoke();
                var config = context.RequestServices.GetRequiredService<EMF.Config.IConfiguration>();
                var logger = context.RequestServices.GetRequiredService<IMetricsLogger>();

                if (!String.IsNullOrEmpty(config.ServiceName)) {
                    logger.SetNamespace(config.ServiceName);
                }

                await action(context, logger);
                stopWatch.Stop();
                logger.PutMetric("Time", stopWatch.ElapsedMilliseconds, Model.Unit.MILLISECONDS);
            });
        }
    }

    public class MetricOptions
    {
        public static MetricOptions Default = new MetricOptions();
    }
}