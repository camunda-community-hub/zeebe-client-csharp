using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using ResolveIncidentResponse = Zeebe.Client.Impl.Responses.ResolveIncidentResponse;

namespace Zeebe.Client.Impl.Commands;

public class ResolveIncidentCommand(
    Gateway.GatewayClient client,
    IAsyncRetryStrategy asyncRetryStrategy,
    long incidentKey)
    : IResolveIncidentCommandStep1
{
    private readonly ResolveIncidentRequest request = new()
    {
        IncidentKey = incidentKey
    };

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