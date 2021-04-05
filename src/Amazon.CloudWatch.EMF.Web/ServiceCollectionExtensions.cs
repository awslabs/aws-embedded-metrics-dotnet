using Amazon.CloudWatch.EMF.Logger;
using Microsoft.Extensions.DependencyInjection;

namespace Amazon.CloudWatch.EMF.Web
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEmf(this IServiceCollection services)
        {
            services.AddScoped<IMetricsLogger, MetricsLogger>();
            services.AddSingleton<EMF.Environment.IEnvironmentProvider, EMF.Environment.EnvironmentProvider>();
            services.AddSingleton<EMF.Environment.IResourceFetcher, EMF.Environment.ResourceFetcher>();
            services.AddSingleton<EMF.Config.IConfiguration>(EMF.Config.EnvironmentConfigurationProvider.Config);
        }
    }
}