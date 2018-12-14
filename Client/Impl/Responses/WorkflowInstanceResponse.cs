using GatewayProtocol;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class WorkflowInstanceResponse : IWorkflowInstanceResponse
    {
        public long WorkflowKey { get; }
        public string BpmnProcessId { get; }
        public int Version { get; }
        public long WorkflowInstanceKey { get; }

        public WorkflowInstanceResponse(CreateWorkflowInstanceResponse response)
        {
            WorkflowKey = response.WorkflowKey;
            BpmnProcessId = response.BpmnProcessId;
            Version = response.Version;
            WorkflowInstanceKey = response.WorkflowInstanceKey;
        }
    }
}