using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class WorkflowMetadata : IWorkflowMetadata
    {
        public string BpmnProcessId { get; }
        public int Version { get; }
        public long WorkflowKey { get; }
        public string ResourceName { get; }

        public WorkflowMetadata(GatewayProtocol.WorkflowMetadata metadata)
        {
            BpmnProcessId = metadata.BpmnProcessId;
            Version = metadata.Version;
            WorkflowKey = metadata.WorkflowKey;
            ResourceName = metadata.ResourceName;
        }
    }
}