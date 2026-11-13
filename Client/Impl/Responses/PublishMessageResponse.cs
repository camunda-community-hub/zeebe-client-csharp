using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses;

public class PublishMessageResponse(GatewayProtocol.PublishMessageResponse response) : IPublishMessageResponse
{
    public long Key { get; } = response.Key;

    public string TenantId { get; } = response.TenantId;
}