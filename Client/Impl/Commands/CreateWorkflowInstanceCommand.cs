using GatewayProtocol;
using System.Threading.Tasks;
using Zeebe.Client.Api.CommandsClient;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client.Impl.Commands
{
    public class CreateWorkflowInstanceCommand : ICreateWorkflowInstanceCommandStep1, ICreateWorkflowInstanceCommandStep2, ICreateWorkflowInstanceCommandStep3
    {
        private const int LatestVersionValue = -1;

        private readonly CreateWorkflowInstanceRequest request;
        private readonly Gateway.GatewayClient client;

        public CreateWorkflowInstanceCommand(Gateway.GatewayClient client)
        {
            this.client = client;
            request = new CreateWorkflowInstanceRequest();
        }

        public ICreateWorkflowInstanceCommandStep2 BpmnProcessId(string bpmnProcessId)
        {
            request.BpmnProcessId = bpmnProcessId;
            return this;
        }

        public ICreateWorkflowInstanceCommandStep3 WorkflowKey(long workflowKey)
        {
            request.WorkflowKey = workflowKey;
            return this;
        }

        public ICreateWorkflowInstanceCommandStep3 Version(int version)
        {
            request.Version = version;
            return this;
        }

        public ICreateWorkflowInstanceCommandStep3 LatestVersion()
        {
            request.Version = LatestVersionValue;
            return this;
        }

        public ICreateWorkflowInstanceCommandStep3 Variables(string variables)
        {
            request.Variables = variables;
            return this;
        }

        public async Task<IWorkflowInstanceResponse> Send()
        {
            var asyncReply = client.CreateWorkflowInstanceAsync(request);
            var response = await asyncReply.ResponseAsync;
            return new WorkflowInstanceResponse(response);
        }
    }
}