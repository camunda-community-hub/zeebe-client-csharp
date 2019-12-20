using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class SetVariablesResponse : ISetVariablesResponse
    {
        public long Key { get; }

        public SetVariablesResponse(GatewayProtocol.SetVariablesResponse response)
        {
            Key = response.Key;
        }
    }
}