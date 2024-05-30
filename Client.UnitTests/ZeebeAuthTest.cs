// 
//     Copyright (c) 2021 camunda services GmbH (info@camunda.com)
// 
//     Licensed under the Apache License, Version 2.0 (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
// 
//         http://www.apache.org/licenses/LICENSE-2.0
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Logging;
using NUnit.Framework;
using Zeebe.Client.Api.Builder;

namespace Zeebe.Client;

[TestFixture]
public class ZeebeAuthTest
{
    private static readonly string ServerCertPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "chain.cert.pem");

    private static readonly string ClientCertPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "chain.cert.pem");

    private static readonly string ServerKeyPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "private.key.pem");

    private static readonly string WrongCertPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "server.crt");

    [SetUp]
    public void Setup()
    {
        // given
        GrpcEnvironment.SetLogger(new ConsoleLogger());

        var keyCertificatePairs = new List<KeyCertificatePair>();
        var serverCert = File.ReadAllText(ServerCertPath);
        keyCertificatePairs.Add(new KeyCertificatePair(serverCert, File.ReadAllText(ServerKeyPath)));
        var channelCredentials = new SslServerCredentials(keyCertificatePairs);

        var server = new Server();
        server.Ports.Add(new ServerPort("localhost", 26505, channelCredentials));

        var testService = new GatewayTestService();
        var serviceDefinition = Gateway.BindService(testService);
        server.Services.Add(serviceDefinition);
        server.Start();
    }

    [Test]
    public async Task ShouldUseTransportEncryption()
    {
        // given
        var zeebeClient = ZeebeClient.Builder()
            .UseGatewayAddress("localhost:26505")
            .UseTransportEncryption(ClientCertPath)
            .AllowUntrustedCertificates()
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
        // given
        var zeebeClient = ZeebeClient.Builder()
            .UseGatewayAddress("localhost:26505")
            .UseTransportEncryption(ServerCertPath)
            .AllowUntrustedCertificates()
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

    class MyInterceptor : Interceptor { }

    [Test]
    public async Task ShouldUseTransportEncryptionWithServerCer2t()
    {
        // given
        var zeebeClient = ZeebeClient.Builder()
            .UseGatewayAddress("localhost:26505")
            .UseTransportEncryption(ServerCertPath)
            .AllowUntrustedCertificates()
            .UseInterceptors(new MyInterceptor())
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
    public async Task ShouldFailOnWrongCert()
    {
        // given
        var zeebeClient = ZeebeClient.Builder()
            .UseGatewayAddress("localhost:26505")
            .UseTransportEncryption(WrongCertPath)
            .Build();

        // when
        try
        {
            await zeebeClient
                .NewPublishMessageCommand()
                .MessageName("messageName")
                .CorrelationKey("p-1")
                .Send();
            Assert.Fail();
        }
        catch (RpcException rpcException)
        {
            // expected
            Assert.AreEqual(rpcException.Status.StatusCode, StatusCode.Internal);
        }
    }

    [Test]
    public async Task ShouldUseAccessToken()
    {
        // given
        var zeebeClient = ZeebeClient.Builder()
            .UseGatewayAddress("localhost:26505")
            .UseTransportEncryption(ClientCertPath)
            .AllowUntrustedCertificates()
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
        // given
        var accessTokenSupplier = new SimpleAccessTokenSupplier();
        var zeebeClient = ZeebeClient.Builder()
            .UseGatewayAddress("localhost:26505")
            .UseTransportEncryption(ClientCertPath)
            .AllowUntrustedCertificates()
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
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Count++;
            return Task.FromResult("token");
        }
    }
}