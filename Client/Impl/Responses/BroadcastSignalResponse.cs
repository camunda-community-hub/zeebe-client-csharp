using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses;

public class BroadcastSignalResponse(GatewayProtocol.BroadcastSignalResponse response) : IBroadcastSignalResponse
{
    public long Key { get; } = response.Key;

    public string TenantId { get; } = response.TenantId;
}