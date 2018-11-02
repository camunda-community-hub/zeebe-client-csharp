using System;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class PartitionInfo : IPartitionInfo
    {
        public int PartitionId { get; set; }
        public bool IsLeader { get; set; }
        public PartitionBrokerRole Role { get; set; }

        public PartitionInfo(GatewayProtocol.Partition partition)
        {
            PartitionId = partition.PartitionId;
            IsLeader = partition.Role == GatewayProtocol.Partition.Types.PartitionBrokerRole.Leader;
            Role = partition.Role == GatewayProtocol.Partition.Types.PartitionBrokerRole.Leader ? PartitionBrokerRole.LEADER : PartitionBrokerRole.FOLLOWER;
        }
    }
}
