using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;

namespace Client.IntegrationTests
{
    [TestFixture]
    public class BrokerTopologyTest
    {
        private readonly ZeebeIntegrationTestHelper testHelper = ZeebeIntegrationTestHelper.Latest();
        private IZeebeClient zeebeClient;

        [OneTimeSetUp]
        public async Task Setup()
        {
            zeebeClient = await testHelper.SetupIntegrationTest();
        }

        [OneTimeTearDown]
        public async Task Stop()
        {
            await testHelper.TearDownIntegrationTest();
        }

        [Test]
        public async Task RequestTopology()
        {
            // given

            // when
            var topology = await zeebeClient.TopologyRequest().Send();

            // then
            Console.WriteLine(topology);

            var gatewayVersion = topology.GatewayVersion;
            Assert.Equals(ZeebeIntegrationTestHelper.LatestVersion, gatewayVersion);

            var topologyBrokers = topology.Brokers;
            Assert.Equals(1, topologyBrokers.Count);

            var topologyBroker = topologyBrokers[0];
            Assert.Equals(0, topologyBroker.NodeId);

            // assert host and port ?!
            // Assert.Equals(container.GetMappedPort(26500), topologyBroker.Port);

            var partitions = topologyBroker.Partitions;
            Assert.Equals(1, partitions.Count);

            var partitionInfo = partitions[0];
            Assert.Equals(PartitionBrokerRole.LEADER, partitionInfo.Role);
            Assert.Equals(true, partitionInfo.IsLeader);
            Assert.Equals(1, partitionInfo.PartitionId);
        }
    }
}