using GatewayProtocol;
using System.Threading.Tasks;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Commands
{
    public class SetVariablesCommand : ISetVariablesCommandStep1, ISetVariablesCommandStep2
    {
        private readonly SetVariablesRequest request;
        private readonly Gateway.GatewayClient client;

        public SetVariablesCommand(Gateway.GatewayClient client, long elementInstanceKey)
        {
            request = new SetVariablesRequest
            {
                ElementInstanceKey = elementInstanceKey
            };
            this.client = client;
        }
        public ISetVariablesCommandStep2 Variables(string variables)
        {
            request.Variables = variables;
            return this;
        }


        public ISetVariablesCommandStep2 Local()
        {
            request.Local = true;
            return this;
        }

        public async Task<ISetVariablesResponse> Send()
        {
            var asyncReply = client.SetVariablesAsync(request);
            await asyncReply.ResponseAsync;
            return new Responses.SetVariablesResponse();
        }
    }
}