using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses;

public class FormMetadata : IFormMetadata
{
    public string FormId { get; }
    public int Version { get; }
    public long FormKey { get; }
    public string ResourceName { get; }
    public string TenantId { get; }

    public FormMetadata(GatewayProtocol.FormMetadata metadata)
    {
        FormId = metadata.FormId;
        Version = metadata.Version;
        FormKey = metadata.FormKey;
        ResourceName = metadata.ResourceName;
        TenantId = metadata.TenantId;
    }
}
