using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Zeebe.Client.Builder;

namespace Zeebe.Client
{

    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Zeebee builders to the ServicesCollection
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddZeebeBuilders(this IServiceCollection services)
        {
            services.AddTransient<IZeebeClientBuilder, ZeebeClientBuilder>();
            services.AddTransient<IZeebeClientTransportBuilder, ZeebeClientBuilder>();
            services.AddTransient<CamundaCloudTokenProviderBuilder>();
            return services;
        }
    }
}
