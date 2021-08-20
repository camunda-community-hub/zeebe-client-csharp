using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations;
using DotNet.Testcontainers.Containers.Modules;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using TestContainers.Core.Builders;
using TestContainers.Core.Containers;
using Zeebe.Client;

namespace Client.IntegrationTests
{
    public class ZeebeIntegrationTestHelper
    {
        public const string LatestVersion = "1.0.0";

        private TestcontainersContainer container;
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

        public ZeebeIntegrationTestHelper withPartitionCount(int count)
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
                .WithImage("camunda/zeebe:" + version)
                .WithPortBinding(26500)
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

            var host = "0.0.0.0:" + container.GetMappedPublicPort(26500);

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