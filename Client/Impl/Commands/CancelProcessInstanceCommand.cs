using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using CancelProcessInstanceResponse = Zeebe.Client.Impl.Responses.CancelProcessInstanceResponse;

namespace Zeebe.Client.Impl.Commands
{
    public class CancelProcessInstanceCommand : ICancelProcessInstanceCommandStep1
    {
        private readonly CancelProcessInstanceRequest request;
        private readonly Gateway.GatewayClient client;
        private readonly IAsyncRetryStrategy asyncRetryStrategy;

        public CancelProcessInstanceCommand(Gateway.GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy, long processInstanceKey)
        {
            request = new CancelProcessInstanceRequest
            {
                ProcessInstanceKey = processInstanceKey
            };
            this.client = client;
            this.asyncRetryStrategy = asyncRetryStrategy;
        }

        public async Task<ICancelProcessInstanceResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
        {
            var asyncReply = client.CancelProcessInstanceAsync(request, deadline: timeout?.FromUtcNow(), cancellationToken: token);
            await asyncReply.ResponseAsync;
            return new CancelProcessInstanceResponse();
        }

        public async Task<ICancelProcessInstanceResponse> Send(CancellationToken cancellationToken)
        {
            return await Send(token: cancellationToken);
        }

        public async Task<ICancelProcessInstanceResponse> SendWithRetry(TimeSpan? timespan = null, CancellationToken token = default)
        {
            return await asyncRetryStrategy.DoWithRetry(() => Send(timespan, token));
        }
    }
}
