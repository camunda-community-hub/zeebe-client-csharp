using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Impl.Builder;

namespace Zeebe.Client
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Zeebe builders to the IServiceCollection
        /// </summary>
        /// <param name="services">the collection where the zeebe services are appended</param>
        /// <returns>the service collection</returns>
        public static IServiceCollection AddZeebeBuilders(this IServiceCollection services)
        {
            services.AddTransient(serviceProvider =>
            {
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                return ZeebeClient.Builder().UseLoggerFactory(loggerFactory);
            });
            services.AddTransient(serviceProvider =>
            {
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                return CamundaCloudTokenProvider.Builder().UseLoggerFactory(loggerFactory);
            });
            return services;
        }
    }
}
