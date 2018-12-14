using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client.Impl.Commands
{
    public class UpdatePayloadCommand : IUpdatePayloadCommandStep1, IUpdatePayloadCommandStep2
    {
        private readonly UpdateWorkflowInstancePayloadRequest request;
        private readonly Gateway.GatewayClient client;

        public UpdatePayloadCommand(Gateway.GatewayClient client, long elementInstanceKey)
        {
            request = new UpdateWorkflowInstancePayloadRequest
            {
                ElementInstanceKey = elementInstanceKey
            };
            this.client = client;
        }

        public IUpdatePayloadCommandStep2 Payload(string payload)
        {
            request.Payload = payload;
            return this;
        }

        public async Task<IUpdatePayloadResponse> Send()
        {
            var asyncReply = client.UpdateWorkflowInstancePayloadAsync(request);
            await asyncReply.ResponseAsync;
            return new UpdatePayloadResponse();
        }
    }
}