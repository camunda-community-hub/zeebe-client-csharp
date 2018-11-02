using System;
namespace Zeebe.Client.Api.Responses
{
    public interface IPartitionInfo
    {
        /** @return the partition's id */
        int PartitionId { get; }

        /** @return the current role of the broker for this partition (i.e. leader or follower) */
        PartitionBrokerRole Role { get; }

        /** @return <code>true</code> if the broker is the current leader of this partition */
        bool IsLeader { get; }
    }
}
