using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Amazon.CloudWatch.EMF.Web
{
    public class Startup
    {
        public Startup(
            IConfiguration configuration,
            IHostEnvironment hostEnv)
        {
            Configuration = configuration;

            EMF.Config.EnvironmentConfigurationProvider.Config = new EMF.Config.Configuration
            {
                ServiceName = "WeatherApp",
                LogGroupName = "/Canary/Dotnet/CloudWatchAgent/Metrics",
                EnvironmentOverride = hostEnv.IsDevelopment()
                    ? EMF.Environment.Environments.Local
                    // Setting this to unknown will cause the SDK to attempt to 
                    // detect the environment. If you know the compute environment
                    // you will be running on, then you can set this yourself.
                    : EMF.Environment.Environments.Unknown
            };
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEmf();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            // Add middleware which will set metric dimensions based on the request routing
            app.UseEmfMiddleware();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
