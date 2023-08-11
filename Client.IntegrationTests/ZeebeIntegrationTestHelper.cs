using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Zeebe.Client;

namespace Client.IntegrationTests
{
    public class ZeebeIntegrationTestHelper
    {
        public const string LatestVersion = "8.2.8";

        private const ushort ZeebePort = 26500;

        private IContainer container;
        private IZeebeClient client;

        private readonly string version;
        private int count = 1;
        public readonly ILoggerFactory LoggerFactory;

        private ZeebeIntegrationTestHelper(string version)
        {
            this.version = version;
            LoggerFactory = new NLogLoggerFactory();
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
            TestcontainersSettings.Logger = LoggerFactory.CreateLogger<ZeebeIntegrationTestHelper>();
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

        private IContainer CreateZeebeContainer()
        {
            return new ContainerBuilder()
                .WithImage(new DockerImage("camunda", "zeebe", version))
                .WithPortBinding(ZeebePort, true)
                .WithEnvironment("ZEEBE_BROKER_CLUSTER_PARTITIONSCOUNT", count.ToString())
                .WithAutoRemove(true)
                .Build();
        }

        private IZeebeClient CreateZeebeClient()
        {
            var loggerFactory = LoggerFactory;
            var host = container.Hostname + ":" + container.GetMappedPublicPort(ZeebePort);

            return ZeebeClient.Builder()
                .UseLoggerFactory(loggerFactory)
                .UseGatewayAddress(host)
                .UsePlainText()
                .Build();
        }

        private async Task AwaitBrokerReadiness()
        {
            var zeebeClient = (ZeebeClient) client;
            await zeebeClient.Connect();
            var topologyErrorLogger = LoggerFactory.CreateLogger<ZeebeIntegrationTestHelper>();
            var ready = false;
            var retries = 0;
            var maxCount = 1_000_000;
            bool continueLoop;
            do
            {
                try
                {
                    var topology = await client.TopologyRequest().Send(TimeSpan.FromSeconds(1));
                    ready = topology.Brokers[0].Partitions.Count >= count;
                    topologyErrorLogger.LogInformation("Requested topology [retries {Retries}], got '{Topology}'", retries, topology);
                }
                catch (Exception e)
                {
                    topologyErrorLogger.LogError(e, "Exception in sending topology");
                    // retry
                }

                continueLoop = !ready && maxCount > retries++;
                if (continueLoop)
                {
                    await Task.Delay(1 * 1000);
                }
            }
            while (continueLoop);
        }
    }
}