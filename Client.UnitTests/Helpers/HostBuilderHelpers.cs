#nullable enable
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Zeebe.Client.Helpers;

public static class HostBuilderHelpers
{
    public static IHost BuildSimpleHost()
    {
        var host = BuildHost(null);

        host.RunAsync();
        return host;
    }

    public static IHost BuildHostWithTls(X509Certificate2 certificate2)
    {
        var host = BuildHost(certificate2);

        host.RunAsync();
        return host;
    }

    private static IHost BuildHost(X509Certificate2? certificate2)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(builder => builder.AddConsole())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 5000,
                        listenOptions =>
                        {
                            if (certificate2 != null)
                            {
                                listenOptions.UseHttps(certificate2);
                            }

                            listenOptions.Protocols = HttpProtocols.Http2;
                        });
                });
                webBuilder.UseStartup<TestServiceStartup>();
            }).Build();
        return host;
    }
}