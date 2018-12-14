using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using CancelWorkflowInstanceResponse = Zeebe.Client.Impl.Responses.CancelWorkflowInstanceResponse;

namespace Zeebe.Client.Impl.Commands
{
    public class CancelWorkflowInstanceCommand : ICancelWorkflowInstanceCommandStep1
    {
        private readonly CancelWorkflowInstanceRequest request;
        private readonly Gateway.GatewayClient client;

        public CancelWorkflowInstanceCommand(Gateway.GatewayClient client, long workflowInstanceKey)
        {
            request = new CancelWorkflowInstanceRequest
            {
                WorkflowInstanceKey = workflowInstanceKey
            };
            this.client = client;
        }

        public async Task<ICancelWorkflowInstanceResponse> Send()
        {
            var asyncReply = client.CancelWorkflowInstanceAsync(request);
            await asyncReply.ResponseAsync;
            return new CancelWorkflowInstanceResponse();
        }
    }
}