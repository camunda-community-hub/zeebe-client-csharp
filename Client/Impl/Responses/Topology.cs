using System;
using System.Collections.Generic;
using System.Linq;
using GatewayProtocol;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class Topology : ITopology
    {
        public IList<IBrokerInfo> Brokers { get; set; }

        public Topology(TopologyResponse response)
        {
            Brokers = response.Brokers.Select(broker => new BrokerInfo(broker)).Cast<IBrokerInfo>().ToList();
        }

    }
}
