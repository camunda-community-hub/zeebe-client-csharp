using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using SetVariablesResponse = Zeebe.Client.Impl.Responses.SetVariablesResponse;

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

        public async Task<ISetVariablesResponse> Send(TimeSpan? timeout = null)
        {
            var asyncReply = client.SetVariablesAsync(request, deadline: timeout?.FromUtcNow());
            await asyncReply.ResponseAsync;
            return new SetVariablesResponse();
        }
    }
}