#nullable enable
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Grpc.Auth;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Builder;

namespace Zeebe.Client.Impl.Builder;

public class ZeebeClientBuilder : IZeebeClientBuilder, IZeebeClientTransportBuilder
{
    private ILoggerFactory? LoggerFactory { get; set; }
    private string? GatewayAddress { get; set; }

    public IZeebeClientBuilder UseLoggerFactory(ILoggerFactory loggerFactory)
    {
        LoggerFactory = loggerFactory;
        return this;
    }

    public IZeebeClientTransportBuilder UseGatewayAddress(string gatewayAddress)
    {
        GatewayAddress = gatewayAddress;
        return this;
    }

    public IZeebeSecureClientBuilder UseTransportEncryption(string rootCertificatePath)
    {
        return new ZeebeSecureClientBuilder(GatewayAddress, rootCertificatePath, LoggerFactory);
    }

    public IZeebeSecureClientBuilder UseTransportEncryption()
    {
        return new ZeebeSecureClientBuilder(GatewayAddress, LoggerFactory);
    }

    public IZeebeClientFinalBuildStep UsePlainText()
    {
        return new ZeebePlainClientBuilder(GatewayAddress, LoggerFactory);
    }
}

internal record ZeebePlainClientBuilder : IZeebeClientFinalBuildStep
{
    private ILoggerFactory? LoggerFactory { get; }
    private TimeSpan? KeepAlive { get; set; }
    private Func<int, TimeSpan>? SleepDurationProvider { get; set; }
    private string Address { get; }

    public ZeebePlainClientBuilder(string? address, ILoggerFactory? loggerFactory = null)
    {
        if (address is null)
        {
            throw new ArgumentNullException(nameof(address), "Address cannot be null when using non secure connection.");
        }

        if (address.StartsWith("https:"))
        {
            throw new ArgumentException(
                $"Expected address '{address}' to start with 'http' when using a non secure connection.");
        }

        Address = address.StartsWith("http") ? address : $"http://{address}";
        LoggerFactory = loggerFactory;
    }

    public IZeebeClientFinalBuildStep UseKeepAlive(TimeSpan keepAlive)
    {
        KeepAlive = keepAlive;
        return this;
    }

    public IZeebeClientFinalBuildStep UseRetrySleepDurationProvider(Func<int, TimeSpan> sleepDurationProvider)
    {
        SleepDurationProvider = sleepDurationProvider;
        return this;
    }

    public IZeebeClient Build()
    {
        return new ZeebeClient(Address, KeepAlive, SleepDurationProvider, LoggerFactory);
    }
}

internal record ZeebeSecureClientBuilder : IZeebeSecureClientBuilder
{
    private ILoggerFactory? LoggerFactory { get; }
    private TimeSpan? KeepAlive { get; set; }
    private Func<int, TimeSpan>? SleepDurationProvider { get; set; }
    private X509Certificate2? Certificate { get; }
    private bool AllowUntrusted { get; set; }
    private string Address { get; }
    private ChannelCredentials Credentials { get; set; }

    public ZeebeSecureClientBuilder(string? address, string certificatePath, ILoggerFactory? loggerFactory = null)
    {
        if (address is null)
        {
            throw new ArgumentNullException(nameof(address), "Address cannot be null when using secure connection.");
        }

        if (address.StartsWith("http:"))
        {
            throw new ArgumentException(
                $"Expected address '{address}' to start with 'https' when using secure connection.");
        }

        Address = address.StartsWith("https") ? address : $"https://{address}";
        LoggerFactory = loggerFactory;
        Certificate = X509Certificate2.CreateFromPem(File.ReadAllText(certificatePath));
        Credentials = new SslCredentials();
    }

    public ZeebeSecureClientBuilder(string? address, ILoggerFactory? loggerFactory = null)
    {
        if (address is null)
        {
            throw new ArgumentNullException(nameof(address), "Address cannot be null when using secure connection.");
        }

        if (address.StartsWith("http:"))
        {
            throw new ArgumentException(
                $"Expected address '{address}' to start with 'https' when using secure connection.");
        }

        Address = address.StartsWith("https") ? address : $"https://{address}";
        LoggerFactory = loggerFactory;
        Credentials = new SslCredentials();
    }

    public IZeebeSecureClientBuilder AllowUntrustedCertificates()
    {
        AllowUntrusted = true;
        return this;
    }

    public IZeebeClientFinalBuildStep UseAccessToken(string accessToken)
    {
        Credentials = ChannelCredentials.Create(Credentials, GoogleGrpcCredentials.FromAccessToken(accessToken));
        return this;
    }

    public IZeebeClientFinalBuildStep UseAccessTokenSupplier(IAccessTokenSupplier supplier)
    {
        Credentials = ChannelCredentials.Create(Credentials, supplier.ToCallCredentials());
        return this;
    }

    public IZeebeClientFinalBuildStep UseKeepAlive(TimeSpan keepAlive)
    {
        KeepAlive = keepAlive;
        return this;
    }

    public IZeebeClientFinalBuildStep UseRetrySleepDurationProvider(Func<int, TimeSpan> sleepDurationProvider)
    {
        SleepDurationProvider = sleepDurationProvider;
        return this;
    }

    public IZeebeClient Build()
    {
        return new ZeebeClient(Address, Credentials, KeepAlive, SleepDurationProvider, LoggerFactory, Certificate, AllowUntrusted);
    }
}