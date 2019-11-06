using GatewayProtocol;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    /// <inheritdoc />
    public class WorkflowInstanceResultResponse : IWorkflowInstanceResult
    {
        /// <inheritdoc/>
        public long WorkflowKey { get; }
        /// <inheritdoc/>
        public string BpmnProcessId { get; }
        /// <inheritdoc/>
        public int Version { get; }
        /// <inheritdoc/>
        public long WorkflowInstanceKey { get; }
        /// <inheritdoc/>
        public string Variables { get; }

        public WorkflowInstanceResultResponse(CreateWorkflowInstanceWithResultResponse response)
        {
            WorkflowKey = response.WorkflowKey;
            BpmnProcessId = response.BpmnProcessId;
            Version = response.Version;
            WorkflowInstanceKey = response.WorkflowInstanceKey;
            Variables = response.Variables;
        }
    }
}