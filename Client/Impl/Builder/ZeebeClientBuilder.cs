using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Grpc.Auth;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Builder;

namespace Zeebe.Client.Impl.Builder
{
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
        private ILoggerFactory LoggerFactory { get; }
        private TimeSpan? KeepAlive { get; set; }
        private Func<int, TimeSpan> SleepDurationProvider { get; set; }

        private string Address { get; }

        public ZeebePlainClientBuilder(string address, ILoggerFactory loggerFactory = null)
        {
            Address = address;
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

    internal class ZeebeSecureClientBuilder : IZeebeSecureClientBuilder
    {
        private ILoggerFactory LoggerFactory { get; }
        private TimeSpan? KeepAlive { get; set; }
        private Func<int, TimeSpan> SleepDurationProvider { get; set; }

        private string Address { get; }

        private ChannelCredentials Credentials { get; set; }

        private X509Certificate2 Certificate { get; }

        // https://learn.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/channel-credentials#combine-channelcredentials-and-callcredentials
        // If you pass any arguments to the SslCredentials constructor, the internal client code throws an exception.
        // The SslCredentials parameter is only included to maintain compatibility with the Grpc.Core package
        public ZeebeSecureClientBuilder(string address, string certificatePath, ILoggerFactory loggerFactory = null)
        {
            Address = address;
            LoggerFactory = loggerFactory;
            Certificate = CreateFromPemFile(certificatePath);
            Credentials = new SslCredentials();
        }

        public ZeebeSecureClientBuilder(string address, ILoggerFactory loggerFactory = null)
        {
            Address = address;
            LoggerFactory = loggerFactory;
            Credentials = new SslCredentials();
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
            return new ZeebeClient(Address, Credentials, KeepAlive, SleepDurationProvider, LoggerFactory, Certificate);
        }

        private X509Certificate2 CreateFromPemFile(string fileName)
        {
            var pem = File.ReadAllText(fileName);
            var certData = GetBytesFromPem(pem, "CERTIFICATE");
            return new X509Certificate2(certData);
        }

        // PEM loading logic from https://stackoverflow.com/a/10498045/11829
        // .NET does not have a built-in API for loading pem files
        private byte[] GetBytesFromPem(string pemString, string section)
        {
            var header = $"-----BEGIN {section}-----";
            var footer = $"-----END {section}-----";

            var start = pemString.IndexOf(header, StringComparison.Ordinal);
            if (start == -1)
            {
                throw new ArgumentException($"cannot find start section {section} in PEM file");
            }

            start += header.Length;
            var end = pemString.IndexOf(footer, start, StringComparison.Ordinal) - start;

            if (end == -1)
            {
                throw new ArgumentException($"cannot find end section {section} in PEM file");
            }

            return Convert.FromBase64String(pemString.Substring(start, end));
        }
    }
}