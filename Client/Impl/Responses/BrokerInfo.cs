using System.Collections.Generic;
using System.Linq;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class BrokerInfo : IBrokerInfo
    {
        private const char AddressSeparator = ':';

        public string Address { get; }
        public string Host { get; }
        public int NodeId { get; }
        public int Port { get; }
        public IList<IPartitionInfo> Partitions { get; }

        public BrokerInfo(GatewayProtocol.BrokerInfo brokerInfo)
        {
            Address = brokerInfo.Host + AddressSeparator + brokerInfo.Port;
            Host = brokerInfo.Host;
            NodeId = brokerInfo.NodeId;
            Port = brokerInfo.Port;

            Partitions = brokerInfo.Partitions
                .Select(partition => new PartitionInfo(partition))
                .Cast<IPartitionInfo>()
                .ToList();
        }
    }
}
