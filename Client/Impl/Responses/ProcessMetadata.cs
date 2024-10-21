using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses;

public class ProcessMetadata(GatewayProtocol.ProcessMetadata metadata) : IProcessMetadata
{
    public string BpmnProcessId { get; } = metadata.BpmnProcessId;
    public int Version { get; } = metadata.Version;
    public long ProcessDefinitionKey { get; } = metadata.ProcessDefinitionKey;
    public string ResourceName { get; } = metadata.ResourceName;
}