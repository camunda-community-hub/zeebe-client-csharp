using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client.Impl.Commands
{
    public class TopologyRequestCommand : ITopologyRequestStep1
    {

        private readonly Gateway.GatewayClient gatewayClient;
        private readonly TopologyRequest request = new TopologyRequest();

        public TopologyRequestCommand(Gateway.GatewayClient client)
        {
            this.gatewayClient = client;
        }

        public async Task<ITopology> Send()
        {
            var asyncReply = gatewayClient.TopologyAsync(request);
            var response = await asyncReply.ResponseAsync;

            return new Topology(response);
        }
    }
}
