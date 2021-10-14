using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using ResolveIncidentResponse = Zeebe.Client.Impl.Responses.ResolveIncidentResponse;

namespace Zeebe.Client.Impl.Commands
{
    public class ResolveIncidentCommand : IResolveIncidentCommandStep1
    {
        private readonly ResolveIncidentRequest request;
        private readonly Gateway.GatewayClient client;
        private readonly IAsyncRetryStrategy asyncRetryStrategy;

        public ResolveIncidentCommand(Gateway.GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy, long incidentKey)
        {
            request = new ResolveIncidentRequest
            {
                IncidentKey = incidentKey
            };
            this.client = client;
            this.asyncRetryStrategy = asyncRetryStrategy;
        }

        public async Task<IResolveIncidentResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
        {
            var asyncReply = client.ResolveIncidentAsync(request, deadline: timeout?.FromUtcNow(), cancellationToken: token);
            await asyncReply.ResponseAsync;
            return new ResolveIncidentResponse();
        }

        public async Task<IResolveIncidentResponse> Send(CancellationToken cancellationToken)
        {
            return await Send(token: cancellationToken);
        }

        public async Task<IResolveIncidentResponse> SendWithRetry(TimeSpan? timespan = null, CancellationToken token = default)
        {
            return await asyncRetryStrategy.DoWithRetry(() => Send(timespan, token));
        }
    }
}