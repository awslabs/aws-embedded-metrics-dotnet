using System;
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
                var endpoint = context.GetEndpoint();
                if (endpoint != null) {
                    var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                    dimensions.AddDimension("Controller", actionDescriptor.ControllerName);
                    dimensions.AddDimension("Action", actionDescriptor.ActionName);
                }
                dimensions.AddDimension("StatusCode", context.Response.StatusCode.ToString());
                logger.SetDimensions(dimensions);
                logger.PutProperty("TraceId", context.TraceIdentifier);
                logger.PutProperty("Path", context.Request.Path);
                return Task.CompletedTask;
            });
        }

        public static void UseEmfMiddleware(this IApplicationBuilder app, Func<HttpContext, IMetricsLogger, Task> action)
        {
            app.Use(async (context, next) =>
            {
                await next.Invoke();
                var logger = context.RequestServices.GetRequiredService<IMetricsLogger>();
                await action(context, logger);
            });
        }
    }

    public class MetricOptions
    {
        public static MetricOptions Default = new MetricOptions();
    }
}