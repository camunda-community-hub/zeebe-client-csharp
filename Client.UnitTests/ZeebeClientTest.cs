using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using Grpc.Core.Logging;
using NLog;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class ZeebeClientTest
    {

        private static readonly string ServerCertPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources","chain.cert.pem");
        private static readonly string ClientCertPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources","ca.cert.pem");
        private static readonly string ServerKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources","private.key.pem");

        private static readonly string WrongCertPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources","server.crt");

        [Test]
        public void ShouldThrowExceptionAfterDispose()
        {
            // given
            var zeebeClient = ZeebeClient.Builder()
                    .UseGatewayAddress("localhost:26500")
                    .UsePlainText()
                    .Build();

            // when
            zeebeClient.Dispose();

            // then
            var aggregateException = Assert.Throws<AggregateException>(
                () => zeebeClient.TopologyRequest().Send().Wait());

            Assert.AreEqual(1, aggregateException.InnerExceptions.Count);

            var  catchedExceptions = aggregateException.InnerExceptions[0];
            Assert.IsTrue(catchedExceptions.Message.Contains("ZeebeClient was already disposed."));
            Assert.IsInstanceOf(typeof(ObjectDisposedException), catchedExceptions);
        }

        [Test]
        public async Task ShouldUseTransportEncryption()
        {
            // given
            GrpcEnvironment.SetLogger(new ConsoleLogger());

            var keyCertificatePairs = new List<KeyCertificatePair>();
            var serverCert = File.ReadAllText(ServerCertPath);
            keyCertificatePairs.Add(new KeyCertificatePair(serverCert,File.ReadAllText(ServerKeyPath)));
            var channelCredentials = new SslServerCredentials(keyCertificatePairs);

            var server = new Server();
            server.Ports.Add(new ServerPort("0.0.0.0", 26505, channelCredentials));

            var testService = new GatewayTestService();
            var serviceDefinition = Gateway.BindService(testService);
            server.Services.Add(serviceDefinition);
            server.Start();


            // client
            var zeebeClient = ZeebeClient.Builder()
                    .UseGatewayAddress("0.0.0.0:26505")
                    .UseTransportEncryption(ClientCertPath)
                    .Build();

            // when
            await zeebeClient
                .NewPublishMessageCommand()
                .MessageName("messageName")
                .CorrelationKey("p-1")
                .Send();

            // then
            while (testService.Requests.Count == 0) ;
            Assert.AreEqual(1, testService.Requests.Count);
        }

        [Test]
        public async Task ShouldUseTransportEncryptionWithServerCert()
        {
            // given
            GrpcEnvironment.SetLogger(new ConsoleLogger());

            var keyCertificatePairs = new List<KeyCertificatePair>();
            var serverCert = File.ReadAllText(ServerCertPath);
            keyCertificatePairs.Add(new KeyCertificatePair(serverCert,File.ReadAllText(ServerKeyPath)));
            var channelCredentials = new SslServerCredentials(keyCertificatePairs);

            var server = new Server();
            server.Ports.Add(new ServerPort("0.0.0.0", 26505, channelCredentials));

            var testService = new GatewayTestService();
            var serviceDefinition = Gateway.BindService(testService);
            server.Services.Add(serviceDefinition);
            server.Start();


            // client
            var zeebeClient = ZeebeClient.Builder()
                .UseGatewayAddress("0.0.0.0:26505")
                .UseTransportEncryption(ServerCertPath)
                .Build();

            // when
            await zeebeClient
                .NewPublishMessageCommand()
                .MessageName("messageName")
                .CorrelationKey("p-1")
                .Send();

            // then
            while (testService.Requests.Count == 0) ;
            Assert.AreEqual(1, testService.Requests.Count);
        }

        [Test]
        public async Task ShouldFailOnWrongCert()
        {
            // given
            GrpcEnvironment.SetLogger(new ConsoleLogger());

            var keyCertificatePairs = new List<KeyCertificatePair>();
            var serverCert = File.ReadAllText(ServerCertPath);
            keyCertificatePairs.Add(new KeyCertificatePair(serverCert,File.ReadAllText(ServerKeyPath)));
            var channelCredentials = new SslServerCredentials(keyCertificatePairs);

            var server = new Server();
            server.Ports.Add(new ServerPort("0.0.0.0", 26505, channelCredentials));

            var testService = new GatewayTestService();
            var serviceDefinition = Gateway.BindService(testService);
            server.Services.Add(serviceDefinition);
            server.Start();


            // client
            var zeebeClient = ZeebeClient.Builder()
                .UseGatewayAddress("0.0.0.0:26505")
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
            catch (Exception e)
            {
                // expected
            }
        }
    }
}