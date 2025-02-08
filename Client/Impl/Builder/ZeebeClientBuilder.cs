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
    private ILoggerFactory LoggerFactory { get; set; }
    private string GatewayAddress { get; set; }

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

internal class ZeebePlainClientBuilder : IZeebeClientFinalBuildStep
{
    private readonly ILoggerFactory loggerFactory;
    private TimeSpan? keepAlive;
    private Func<int, TimeSpan> sleepDurationProvider;

    public ZeebePlainClientBuilder(string address, ILoggerFactory loggerFactory = null)
    {
        if (address.StartsWith("https:"))
            throw new ArgumentException(
                $@"Expected address '{address}' to start with 'http' when using a non secure connection.");

        Address = address.StartsWith("http") ? address : $"http://{address}";
        this.loggerFactory = loggerFactory;
    }

    private string Address { get; }

    public IZeebeClientFinalBuildStep UseKeepAlive(TimeSpan keepAlive)
    {
        this.keepAlive = keepAlive;
        return this;
    }

    public IZeebeClientFinalBuildStep UseRetrySleepDurationProvider(Func<int, TimeSpan> sleepDurationProvider)
    {
        this.sleepDurationProvider = sleepDurationProvider;
        return this;
    }

    public IZeebeClient Build()
    {
        return new ZeebeClient(Address, keepAlive, sleepDurationProvider, loggerFactory);
    }
}

internal class ZeebeSecureClientBuilder : IZeebeSecureClientBuilder
{
    private readonly ILoggerFactory loggerFactory;
    private bool allowUntrusted;
    private readonly X509Certificate2 certificate;
    private TimeSpan? keepAlive;
    private Func<int, TimeSpan> sleepDurationProvider;

    public ZeebeSecureClientBuilder(string address, string certificatePath, ILoggerFactory loggerFactory = null)
    {
        if (address.StartsWith("http:"))
            throw new ArgumentException(
                $"Expected address '{address}' to start with 'https' when using secure connection.");

        Address = address.StartsWith("https") ? address : $"https://{address}";
        this.loggerFactory = loggerFactory;
        certificate = X509Certificate2.CreateFromPem(File.ReadAllText(certificatePath));
        Credentials = new SslCredentials();
    }

    public ZeebeSecureClientBuilder(string address, ILoggerFactory loggerFactory = null)
    {
        if (address.StartsWith("http:"))
            throw new ArgumentException(
                $"Expected address '{address}' to start with 'https' when using secure connection.");

        Address = address.StartsWith("https") ? address : $"https://{address}";
        this.loggerFactory = loggerFactory;
        Credentials = new SslCredentials();
    }

    private string Address { get; }

    private ChannelCredentials Credentials { get; set; }

    public IZeebeSecureClientBuilder AllowUntrustedCertificates()
    {
        allowUntrusted = true;
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
        this.keepAlive = keepAlive;
        return this;
    }

    public IZeebeClientFinalBuildStep UseRetrySleepDurationProvider(Func<int, TimeSpan> sleepDurationProvider)
    {
        this.sleepDurationProvider = sleepDurationProvider;
        return this;
    }

    public IZeebeClient Build()
    {
        return new ZeebeClient(Address, Credentials, keepAlive, sleepDurationProvider, loggerFactory, certificate,
            allowUntrusted);
    }
}