using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client.Impl.Commands
{
    public class ListWorkflowRequest : IListWorkflowsRequestStep1, IListWorkflowsRequestStep2
    {
        private readonly ListWorkflowsRequest request;
        private readonly Gateway.GatewayClient client;

        public ListWorkflowRequest(Gateway.GatewayClient client)
        {
            this.client = client;
            request = new ListWorkflowsRequest();
        }

        public IListWorkflowsRequestStep2 BpmnProcessId(string bpmnProcessId)
        {
            request.BpmnProcessId = bpmnProcessId;
            return this;
        }

        public async Task<IWorkflowListResponse> Send()
        {
            var asyncReply = client.ListWorkflowsAsync(request);
            var response = await asyncReply.ResponseAsync;
            return new WorkflowListResponse(response);
        }
    }
}