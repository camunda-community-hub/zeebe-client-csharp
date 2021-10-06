using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client.Impl.Commands
{
    public class UpdateRetriesCommand : IUpdateRetriesCommandStep1, IUpdateRetriesCommandStep2
    {
        private readonly UpdateJobRetriesRequest request;
        private readonly Gateway.GatewayClient client;
        private readonly IAsyncRetryStrategy asyncRetryStrategy;

        public UpdateRetriesCommand(Gateway.GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy, long jobKey)
        {
            request = new UpdateJobRetriesRequest
            {
                JobKey = jobKey
            };
            this.client = client;
            this.asyncRetryStrategy = asyncRetryStrategy;
        }

        public IUpdateRetriesCommandStep2 Retries(int retries)
        {
            request.Retries = retries;
            return this;
        }

        public async Task<IUpdateRetriesResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
        {
            var asyncReply = client.UpdateJobRetriesAsync(request, deadline: timeout?.FromUtcNow(), cancellationToken: token);
            await asyncReply.ResponseAsync;
            return new UpdateRetriesResponse();
        }

        public async Task<IUpdateRetriesResponse> Send(CancellationToken cancellationToken)
        {
            return await Send(token: cancellationToken);
        }

        public async Task<IUpdateRetriesResponse> SendWithRetry(TimeSpan? timeout = null, CancellationToken token = default)
        {
            return await asyncRetryStrategy.DoWithRetry(() => Send(timeout, token));
        }
    }
}