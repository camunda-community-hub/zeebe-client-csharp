using GatewayProtocol;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class WorkflowInstanceResponse : IWorkflowInstanceResponse
    {
        /// <inheritdoc/>
        public long WorkflowKey { get; }
        /// <inheritdoc/>
        public string BpmnProcessId { get; }
        /// <inheritdoc/>
        public int Version { get; }
        /// <inheritdoc/>
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