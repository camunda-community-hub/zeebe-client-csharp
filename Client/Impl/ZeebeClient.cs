using System.Threading.Tasks;
using Grpc.Core;
using GatewayProtocol;
using Zeebe.Client.Api.Clients;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Impl.Commands;

namespace Zeebe.Client.Impl
{
    public class ZeebeClient : IZeebeClient
    {
        private Channel channelToGateway;
        private Gateway.GatewayClient gatewayClient;

        public ZeebeClient(string address)
        {
            channelToGateway = new Channel(address, ChannelCredentials.Insecure);
            gatewayClient = new Gateway.GatewayClient(channelToGateway);
        }

        public IJobClient JobClient() => new JobClient(gatewayClient);

        public ITopologyRequestStep1 TopologyRequest() => new TopologyRequestCommand(gatewayClient);

        public IWorkflowClient WorkflowClient() => new WorkflowClient(gatewayClient);

        public void Dispose() => channelToGateway.ShutdownAsync();
    }
}
