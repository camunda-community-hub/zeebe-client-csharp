using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client.Impl.Commands
{
    /// <inheritdoc />
    public class CreateProcessInstanceCommandWithResult : ICreateProcessInstanceWithResultCommandStep1
    {
        private static readonly long DefaultGatewayBrokerTimeoutMillisecond = 20 * 1000;
        private static readonly long DefaultTimeoutAdditionMillisecond = 10 * 1000;

        private readonly CreateProcessInstanceWithResultRequest createWithResultRequest;
        private readonly Gateway.GatewayClient client;
        private readonly IAsyncRetryStrategy asyncRetryStrategy;

        public CreateProcessInstanceCommandWithResult(Gateway.GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy, CreateProcessInstanceRequest createRequest)
        {
            this.client = client;
            createWithResultRequest = new CreateProcessInstanceWithResultRequest { Request = createRequest };
            this.asyncRetryStrategy = asyncRetryStrategy;
        }

        /// <inheritdoc/>
        public ICreateProcessInstanceWithResultCommandStep1 FetchVariables(IList<string> fetchVariables)
        {
            createWithResultRequest.FetchVariables.AddRange(fetchVariables);
            return this;
        }

        /// <inheritdoc/>
        public ICreateProcessInstanceWithResultCommandStep1 FetchVariables(params string[] fetchVariables)
        {
            createWithResultRequest.FetchVariables.AddRange(fetchVariables);
            return this;
        }

        /// <inheritdoc/>
        public async Task<IProcessInstanceResult> Send(TimeSpan? timeout = null, CancellationToken token = default)
        {
            // this timeout will be used for the Gateway-Broker communication
            createWithResultRequest.RequestTimeout = (long)(timeout?.TotalMilliseconds ?? DefaultGatewayBrokerTimeoutMillisecond);

            // this is the timeout between client and gateway
            var clientDeadline = TimeSpan.FromMilliseconds(createWithResultRequest.RequestTimeout +
                                                           DefaultTimeoutAdditionMillisecond).FromUtcNow();

            var asyncReply = client.CreateProcessInstanceWithResultAsync(createWithResultRequest, deadline: clientDeadline, cancellationToken: token);
            var response = await asyncReply.ResponseAsync;
            return new ProcessInstanceResultResponse(response);
        }

        public async Task<IProcessInstanceResult> Send(CancellationToken cancellationToken)
        {
                return await Send(token: cancellationToken);
        }

        public async Task<IProcessInstanceResult> SendWithRetry(TimeSpan? timespan = null, CancellationToken token = default)
        {
            return await asyncRetryStrategy.DoWithRetry(() => Send(timespan, token));
        }
    }
}
