using System;
using System.Collections.Generic;
using System.Linq;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class BrokerInfo : IBrokerInfo
    {
        public string Address { get; set; }
        public string Host { get; set; }
        public int NodeId { get; set; }
        public int Port { get; set; }
        public IList<IPartitionInfo> Partitions { get; set; }

        public BrokerInfo(GatewayProtocol.BrokerInfo brokerInfo)
        {
            Address = brokerInfo.Host + brokerInfo.Port;
            Host = brokerInfo.Host;
            NodeId = brokerInfo.NodeId;
            Port = brokerInfo.Port;

            Partitions = brokerInfo.Partitions
                .Select(parition => new PartitionInfo(parition))
                .Cast<IPartitionInfo>()
                .ToList();
        }
    }
}
