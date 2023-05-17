using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Zeebe.Client.Helpers;

public sealed class TestServiceStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddGrpc(o => o.EnableDetailedErrors = true);
        services.AddSingleton<IRequestHandlerRegistration, RequestHandlerRegistration>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints => { endpoints.MapGrpcService<GatewayTestService>(); });
    }
}