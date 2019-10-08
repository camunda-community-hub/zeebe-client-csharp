//
//    Copyright (c) 2018 camunda services GmbH (info@camunda.com)
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
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

        public override string ToString()
        {
            return $"\n {nameof(Address)}: {Address}," +
                   $"\n {nameof(Host)}: {Host}," +
                   $"\n {nameof(NodeId)}: {NodeId}," +
                   $"\n {nameof(Port)}: {Port}," +
                   "\n Partitions:\n" + string.Join(", ", Partitions);
        }
    }
}
