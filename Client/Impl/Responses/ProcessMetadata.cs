using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class ProcessMetadata : IProcessMetadata
    {
        public string BpmnProcessId { get; }
        public int Version { get; }
        public long ProcessDefinitionKey { get; }
        public string ResourceName { get; }

        public ProcessMetadata(GatewayProtocol.ProcessMetadata metadata)
        {
            BpmnProcessId = metadata.BpmnProcessId;
            Version = metadata.Version;
            ProcessDefinitionKey = metadata.ProcessDefinitionKey;
            ResourceName = metadata.ResourceName;
        }
    }
}
