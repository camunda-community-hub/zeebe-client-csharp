using GatewayProtocol;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class WorkflowResourceResponse : IWorkflowResourceResponse
    {
        public string BpmnXml { get; }

        public int Version { get; }

        public string BpmnProcessId { get; }

        public string ResourceName { get; }

        public long WorkflowKey { get; }

        public WorkflowResourceResponse(GetWorkflowResponse response)
        {
            BpmnXml = response.BpmnXml;
            Version = response.Version;
            BpmnProcessId = response.BpmnProcessId;
            ResourceName = response.ResourceName;
            WorkflowKey = response.WorkflowKey;
        }

        public override string ToString()
        {
            return $"{nameof(BpmnXml)}: {BpmnXml}, {nameof(Version)}: {Version}, {nameof(BpmnProcessId)}: {BpmnProcessId}, {nameof(ResourceName)}: {ResourceName}, {nameof(WorkflowKey)}: {WorkflowKey}";
        }
    }
}