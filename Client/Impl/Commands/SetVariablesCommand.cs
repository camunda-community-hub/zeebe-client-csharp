using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using SetVariablesResponse = Zeebe.Client.Impl.Responses.SetVariablesResponse;

namespace Zeebe.Client.Impl.Commands
{
    public class SetVariablesCommand : ISetVariablesCommandStep1, ISetVariablesCommandStep2
    {
        private readonly SetVariablesRequest request;
        private readonly Gateway.GatewayClient client;
        private readonly IAsyncRetryStrategy asyncRetryStrategy;

        public SetVariablesCommand(Gateway.GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy, long elementInstanceKey)
        {
            request = new SetVariablesRequest
            {
                ElementInstanceKey = elementInstanceKey
            };
            this.client = client;
            this.asyncRetryStrategy = asyncRetryStrategy;
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

        public async Task<ISetVariablesResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
        {
            var asyncReply = client.SetVariablesAsync(request, deadline: timeout?.FromUtcNow(), cancellationToken: token);
            var response = await asyncReply.ResponseAsync;
            return new SetVariablesResponse(response);
        }

        public async Task<ISetVariablesResponse> Send(CancellationToken cancellationToken)
        {
            return await Send(token: cancellationToken);
        }

        public async Task<ISetVariablesResponse> SendWithRetry(TimeSpan? timespan = null, CancellationToken token = default)
        {
            return await asyncRetryStrategy.DoWithRetry(() => Send(timespan, token));
        }
    }
}