using NUnit.Framework;
using GatewayProtocol;
using Zeebe.Client.Api.Responses;
using System.Threading.Tasks;

namespace Zeebe.Client
{
    [TestFixture]
    public class RequestTopologyTest : BaseZeebeTest
    {
        [Test]
        public async Task ShouldSendRequestAsExpected()
        {
            // given
            TopologyRequest expectedRequest = new TopologyRequest();

            // when
            await ZeebeClient.TopologyRequest().Send();

            // then
            var actualRequest = TestService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }


        [Test]
        public async Task ShouldReceiveResponseAsExpected()
        {
            // given
            TopologyResponse expectedResponse = new TopologyResponse();
            expectedResponse.Brokers.Add(CreateBrokerInfo(0, "host", 26501, 0, true));
            expectedResponse.Brokers.Add(CreateBrokerInfo(1, "host", 26501, 0, false));
            expectedResponse.Brokers.Add(CreateBrokerInfo(2, "host", 26501, 0, false));
            TestService.AddRequestHandler(typeof(TopologyRequest), (request) => expectedResponse);

            // when
            ITopology response = await ZeebeClient.TopologyRequest().Send();

            // then
            IBrokerInfo firstBroker = response.Brokers[0];
            Assert.AreEqual("host0:26501", firstBroker.Address);
            Assert.AreEqual(0, firstBroker.NodeId);

            IPartitionInfo firstPartition = firstBroker.Partitions[0];
            Assert.AreEqual(0, firstPartition.PartitionId);
            Assert.AreEqual(PartitionBrokerRole.LEADER, firstPartition.Role);


            IBrokerInfo secondBroker = response.Brokers[1];
            Assert.AreEqual("host1:26501", secondBroker.Address);
            Assert.AreEqual(1, secondBroker.NodeId);

            firstPartition = secondBroker.Partitions[0];
            Assert.AreEqual(0, firstPartition.PartitionId);
            Assert.AreEqual(PartitionBrokerRole.FOLLOWER, firstPartition.Role);


            IBrokerInfo thirdBroker = response.Brokers[2];
            Assert.AreEqual("host2:26501", thirdBroker.Address);
            Assert.AreEqual(2, thirdBroker.NodeId);

            firstPartition = thirdBroker.Partitions[0];
            Assert.AreEqual(0, firstPartition.PartitionId);
            Assert.AreEqual(PartitionBrokerRole.FOLLOWER, firstPartition.Role);

        }


        private GatewayProtocol.BrokerInfo CreateBrokerInfo(int nodeId, string host, int port, int partitionId, bool leader)
        {
            var brokerInfo = new GatewayProtocol.BrokerInfo
            {
                Host = host + nodeId, NodeId = nodeId, Port = port
            };

            var partition = new Partition
            {
                PartitionId = partitionId,
                Role = leader
                    ? Partition.Types.PartitionBrokerRole.Leader
                    : Partition.Types.PartitionBrokerRole.Follower
            };
            brokerInfo.Partitions.Add(partition);

            return brokerInfo;
        }
    }
}
