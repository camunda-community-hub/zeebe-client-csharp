using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using UpdateJobTimeoutResponse = Zeebe.Client.Impl.Responses.UpdateJobTimeoutResponse;

namespace Zeebe.Client.Impl.Commands;

public class UpdateJobTimeoutCommand : IUpdateJobTimeoutCommandStep1, IUpdateJobTimeoutCommandStep2
{
    private readonly UpdateJobTimeoutRequest request;
    private readonly Gateway.GatewayClient client;
    private readonly IAsyncRetryStrategy asyncRetryStrategy;

    public UpdateJobTimeoutCommand(Gateway.GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy, long jobKey)
    {
        request = new UpdateJobTimeoutRequest()
        {
            JobKey = jobKey
        };
        this.client = client;
        this.asyncRetryStrategy = asyncRetryStrategy;
    }

    public IUpdateJobTimeoutCommandStep2 Timeout(TimeSpan timeout)
    {
        request.Timeout = Convert.ToInt32(timeout.TotalMilliseconds);
        return this;
    }

    public async Task<IUpdateJobTimeoutResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
    {
        var asyncReply = client.UpdateJobTimeoutAsync(request, deadline: timeout?.FromUtcNow(), cancellationToken: token);
        await asyncReply.ResponseAsync;
        return new UpdateJobTimeoutResponse();
    }

    public async Task<IUpdateJobTimeoutResponse> Send(CancellationToken cancellationToken)
    {
        return await Send(token: cancellationToken);
    }

    public async Task<IUpdateJobTimeoutResponse> SendWithRetry(TimeSpan? timeout = null, CancellationToken token = default)
    {
        return await asyncRetryStrategy.DoWithRetry(() => Send(timeout, token));
    }
}