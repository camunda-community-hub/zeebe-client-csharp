using System;
using System.Collections.Generic;

namespace Zeebe.Client.Api.Responses
{
    public interface IBrokerInfo
    {
        /** @return the node if of the broker */
        int NodeId { get; }

        /** @return the address host of the broker */
        string Host { get; }

        /** @return the address port of the broker */
        int Port { get; }

        /** @return the address (host+port) of the broker */
        string Address { get; }

        /** @return all partitions of the broker */
        IList<IPartitionInfo> Partitions { get; }
    }
}
