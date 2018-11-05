using NUnit.Framework;
using GatewayProtocol;
using Zeebe.Client.Impl.Responses;
using Zeebe.Client.Api.Responses;
using System.Diagnostics.Contracts;

namespace Zeebe.Client
{
    [TestFixture]
    public class RequestTopologyTest : BaseZeebeTest
    {
        [Test]
        public async void ShouldSendRequestAsExpected()
        {
            // given
            TopologyRequest expectedRequest = new TopologyRequest();

            // when
            ITopology topology = await ZeebeClient.TopologyRequest().Send();

            // then
            var actualRequest = TestService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }


        [Test]
        public async void ShouldReceiveResponseAsExpected()
        {
            // given
            TopologyResponse expectedResponse = new TopologyResponse();
            expectedResponse.Brokers.Add(CreateBrokerInfo(0, "host", 26501, 0, true));
            expectedResponse.Brokers.Add(CreateBrokerInfo(1, "host", 26501, 0, false));
            expectedResponse.Brokers.Add(CreateBrokerInfo(2, "host", 26501, 0, false));
            TestService.AddRequestHandler(typeof(GatewayProtocol.TopologyRequest), (request) => expectedResponse);

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
            GatewayProtocol.BrokerInfo brokerInfo = new GatewayProtocol.BrokerInfo();
            brokerInfo.Host = host + nodeId;
            brokerInfo.NodeId = nodeId;
            brokerInfo.Port = port;

            GatewayProtocol.Partition partition = new GatewayProtocol.Partition();
            partition.PartitionId = partitionId;
            partition.Role = leader ? Partition.Types.PartitionBrokerRole.Leader : Partition.Types.PartitionBrokerRole.Follower;
            brokerInfo.Partitions.Add(partition);

            return brokerInfo;
        }
    }
}
