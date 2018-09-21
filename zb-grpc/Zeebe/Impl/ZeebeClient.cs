using System;
using System.Threading.Tasks;
using Grpc.Core;
using GatewayProtocol;

namespace Zeebe.Impl
{
    public class ZeebeClient
    {
        private HealthRequest healtRequest = new HealthRequest();

        private Channel channelToGateway;
        private Gateway.GatewayClient gatewayClient;

        public ZeebeClient(string address)
        {
            channelToGateway = new Channel(address, ChannelCredentials.Insecure);
            gatewayClient = new Gateway.GatewayClient(channelToGateway);
        }


        public async Task<HealthResponse> HealtRequest()
        {
            var asyncReply = gatewayClient.HealthAsync(healtRequest);
            var response = await asyncReply.ResponseAsync;
            return response;
        }

        public void PublishMessage()
        {
            var publishMessageRequest = new PublishMessageRequest();

            publishMessageRequest.CorrelationKey = "key";
            publishMessageRequest.Name = "messageName";
            publishMessageRequest.Payload = "payload";
            publishMessageRequest.TimeToLive = 1000;
            publishMessageRequest.Payload = "{}";

            var asyncReply = gatewayClient.PublishMessage(publishMessageRequest);
        }
    }
}
