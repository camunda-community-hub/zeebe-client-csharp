using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TestContainers.Core.Builders;
using TestContainers.Core.Containers;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;

namespace Client.IntegrationTests
{
    public class BrokerTest
    {
        private Container container;
        private IZeebeClient client;

        [OneTimeSetUp]
        public async Task Setup()
        {
            container = CreateZeebeContainer();
            await container.Start();

            client = CreateZeebeClient();

            await AwaitBrokerReadiness();
        }

        [OneTimeTearDown]
        public async Task Stop()
        {
            await container.Stop();
        }

        [Test]
        public async Task RequestTopology()
        {
            // given

            // when
            var topology = await client.TopologyRequest().Send();

            // then
            Console.WriteLine(topology);

            var topologyBrokers = topology.Brokers;
            Assert.AreEqual(1, topologyBrokers.Count);

            var topologyBroker = topologyBrokers[0];
            Assert.AreEqual(0, topologyBroker.NodeId);

            // assert host and port ?!
            // Assert.AreEqual(container.GetMappedPort(26500), topologyBroker.Port);

            var partitions = topologyBroker.Partitions;
            Assert.AreEqual(1, partitions.Count);

            var partitionInfo = partitions[0];
            Assert.AreEqual(PartitionBrokerRole.LEADER, partitionInfo.Role);
            Assert.AreEqual(true, partitionInfo.IsLeader);
            Assert.AreEqual(1, partitionInfo.PartitionId);
        }

        private IZeebeClient CreateZeebeClient()
        {
            var host = "0.0.0.0:" + container.GetMappedPort(26500);

            return ZeebeClient.Builder()
                .UseGatewayAddress(host)
                .UsePlainText()
                .Build();
        }

        private Container CreateZeebeContainer()
        {
            return new GenericContainerBuilder<Container>()
                .Begin()
                .WithImage("camunda/zeebe:latest")
                .WithExposedPorts(26500)
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
                    ready = topology.Brokers[0].Partitions.Count == 1;
                }
                catch (Exception e)
                {
                    // retry
                }
            }
            while (!ready);
        }
    }
}