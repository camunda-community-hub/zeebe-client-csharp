using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses;

public class FormMetadata(GatewayProtocol.FormMetadata metadata) : IFormMetadata
{
    public string FormId { get; } = metadata.FormId;
    public int Version { get; } = metadata.Version;
    public long FormKey { get; } = metadata.FormKey;
    public string ResourceName { get; } = metadata.ResourceName;
    public string TenantId { get; } = metadata.TenantId;
}