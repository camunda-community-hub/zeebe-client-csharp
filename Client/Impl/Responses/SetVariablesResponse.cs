using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses;

public class SetVariablesResponse(GatewayProtocol.SetVariablesResponse response) : ISetVariablesResponse
{
    public long Key { get; } = response.Key;
}