using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client.Impl.Commands
{
    public class WorkflowResourceRequest : IWorkflowResourceRequestStep1, IWorkflowResourceRequestStep2, IWorkflowResourceRequestStep3
    {
        private const int LatestVersionValue = -1;

        private readonly GetWorkflowRequest request;
        private readonly Gateway.GatewayClient client;

        public WorkflowResourceRequest(Gateway.GatewayClient client)
        {
            this.client = client;
            request = new GetWorkflowRequest();
        }

        public IWorkflowResourceRequestStep2 BpmnProcessId(string bpmnProcessId)
        {
            request.BpmnProcessId = bpmnProcessId;
            return this;
        }

        public IWorkflowResourceRequestStep3 WorkflowKey(long workflowKey)
        {
            request.WorkflowKey = workflowKey;
            return this;
        }

        public IWorkflowResourceRequestStep3 Version(int version)
        {
            request.Version = version;
            return this;
        }

        public IWorkflowResourceRequestStep3 LatestVersion()
        {
            request.Version = LatestVersionValue;
            return this;
        }

        public async Task<IWorkflowResourceResponse> Send()
        {
            var asyncReply = client.GetWorkflowAsync(request);
            var response = await asyncReply.ResponseAsync;
            return new WorkflowResourceResponse(response);
        }
    }
}