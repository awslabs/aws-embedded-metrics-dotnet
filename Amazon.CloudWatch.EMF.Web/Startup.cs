using Amazon.CloudWatch.EMF.Logger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Amazon.CloudWatch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Amazon.CloudWatch.EMF.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            EMF.Config.EnvironmentConfigurationProvider.Config = new EMF.Config.Configuration
            {
                LogGroupName = "/Canary/Dotnet/CloudWatchAgent/Metrics",
                EnvironmentOverride = EMF.Environment.Environments.Local
            };
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddScoped<IMetricsLogger, MetricsLogger>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            // Add middleware which will set metric dimensions based on the request routing
            app.Use(async (context, next) =>
            {
                await next.Invoke();
                var logger = context.RequestServices.GetRequiredService<IMetricsLogger>();
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
            });

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
