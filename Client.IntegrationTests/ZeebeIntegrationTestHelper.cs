using System;
using System.IO;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Zeebe.Client;

namespace Client.IntegrationTests
{
    public class ZeebeIntegrationTestHelper
    {
        public const string LatestVersion = "1.2.4";

        public const ushort ZeebePort = 26500;

        private ITestcontainersContainer container;
        private IZeebeClient client;

        private readonly string version;
        private int count = 1;

        private ZeebeIntegrationTestHelper(string version)
        {
            this.version = version;
        }

        public static ZeebeIntegrationTestHelper Latest()
        {
            return new ZeebeIntegrationTestHelper(LatestVersion);
        }

        public ZeebeIntegrationTestHelper WithPartitionCount(int count)
        {
            this.count = count;
            return this;
        }

        public static ZeebeIntegrationTestHelper OfVersion(string version)
        {
            return new ZeebeIntegrationTestHelper(version);
        }

        public async Task<IZeebeClient> SetupIntegrationTest()
        {
            container = CreateZeebeContainer();
            await container.StartAsync();

            client = CreateZeebeClient();
            await AwaitBrokerReadiness();
            return client;
        }

        public async Task TearDownIntegrationTest()
        {
            client.Dispose();
            client = null;
            await container.StopAsync();
            container = null;
        }

        private TestcontainersContainer CreateZeebeContainer()
        {
            return new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage(new DockerImage("camunda", "zeebe", version))
                .WithPortBinding(ZeebePort, true)
                .WithEnvironment("ZEEBE_BROKER_CLUSTER_PARTITIONSCOUNT", count.ToString())
                .Build();
        }

        private IZeebeClient CreateZeebeClient()
        {
            var loggerFactory = LoggerFactory.Create(loggingBuilder =>
            {
                // configure Logging with NLog
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "NLog.config");
                loggingBuilder.AddNLog(path);
            });

            var host = container.Hostname + ":" + container.GetMappedPublicPort(ZeebePort);

            return ZeebeClient.Builder()
                .UseLoggerFactory(loggerFactory)
                .UseGatewayAddress(host)
                .UsePlainText()
                .Build();
        }

        private async Task AwaitBrokerReadiness()
        {
            var ready = false;
            do
            {
                try
                {
                    var topology = await client.TopologyRequest().Send();
                    ready = topology.Brokers[0].Partitions.Count >= count;
                }
                catch (Exception)
                {
                    // retry
                }
            }
            while (!ready);
        }
    }
}