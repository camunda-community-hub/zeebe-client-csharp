using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Zeebe.Client.Api.Builder;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Helpers;

namespace Zeebe.Client;

[TestFixture]
public class ZeebeClientTest
{
    [OneTimeSetUp]
    public void Init()
    {
        var certificate = CertificateHelpers.CreateFromPem(ServerCertPath, ServerKeyPath);
        host = HostBuilderHelpers.BuildHostWithTls(certificate);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await host.StopAsync();
    }

    private static readonly string ServerCertPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "chain.cert.pem");

    private static readonly string ClientCertPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "chain.cert.pem");

    private static readonly string ServerKeyPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "private.key.pem");

    private static readonly string WrongCertPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "server.crt");

    private IHost host;

    [Test]
    public async Task ShouldUseTransportEncryption()
    {
        // client
        var zeebeClient = ZeebeClient.Builder()
            .UseGatewayAddress("https://localhost:5000")
            .UseTransportEncryption()
            .Build();

        // when
        var publishMessageResponse = await zeebeClient
            .NewPublishMessageCommand()
            .MessageName("messageName")
            .CorrelationKey("p-1")
            .Send();

        // then
        Assert.NotNull(publishMessageResponse);
    }

    [Test]
    public async Task ShouldUseTransportEncryptionWithServerCert()
    {
        // client
        var zeebeClient = ZeebeClient.Builder()
            .UseGatewayAddress("https://localhost:5000")
            .UseTransportEncryption(ServerCertPath)
            .Build();

        // when
        var publishMessageResponse = await zeebeClient
            .NewPublishMessageCommand()
            .MessageName("messageName")
            .CorrelationKey("p-1")
            .Send();

        // then
        Assert.NotNull(publishMessageResponse);
    }

    [Test]
    [Ignore("since we accept untrusted certificates for other tests")]
    public async Task ShouldFailOnWrongCert()
    {
        // client
        var zeebeClient = ZeebeClient.Builder()
            .UseGatewayAddress("https://localhost:5000")
            .UseTransportEncryption(WrongCertPath)
            .Build();

        // when
        try
        {
            var rc = await zeebeClient
                .NewPublishMessageCommand()
                .MessageName("messageName")
                .CorrelationKey("p-1")
                .Send();
            Assert.Fail();
        }
        catch (RpcException rpcException)
        {
            // expected
            Assert.AreEqual(rpcException.Status.StatusCode, StatusCode.Unavailable);
        }
    }

    [Test]
    public async Task ShouldUseAccessToken()
    {
        // client
        var zeebeClient = ZeebeClient.Builder()
            .UseGatewayAddress("https://localhost:5000")
            .UseTransportEncryption(ClientCertPath)
            .UseAccessToken("token")
            .Build();

        // when
        await zeebeClient.TopologyRequest().Send();
        await zeebeClient.TopologyRequest().Send();
        var topology = await zeebeClient.TopologyRequest().Send();

        // then
        Assert.NotNull(topology);
    }

    [Test]
    public async Task ShouldUseAccessTokenSupplier()
    {
        // client
        var accessTokenSupplier = new SimpleAccessTokenSupplier();
        var zeebeClient = ZeebeClient.Builder()
            .UseGatewayAddress("https://localhost:5000")
            .UseTransportEncryption(ClientCertPath)
            .UseAccessTokenSupplier(accessTokenSupplier)
            .Build();

        // when
        await zeebeClient.TopologyRequest().Send();
        await zeebeClient.TopologyRequest().Send();
        var topology = await zeebeClient.TopologyRequest().Send();

        // then
        Assert.NotNull(topology);
        Assert.AreEqual(3, accessTokenSupplier.Count);
    }

    private class SimpleAccessTokenSupplier : IAccessTokenSupplier
    {
        public int Count { get; private set; }

        public Task<string> GetAccessTokenForRequestAsync(
            string authUri = null,
            CancellationToken cancellationToken = default)
        {
            Count++;
            return Task.FromResult("token");
        }
    }
}