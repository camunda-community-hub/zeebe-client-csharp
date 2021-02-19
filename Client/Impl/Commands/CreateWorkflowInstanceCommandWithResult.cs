using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client.Impl.Commands
{
    /// <inheritdoc />
    public class CreateWorkflowInstanceCommandWithResult : ICreateWorkflowInstanceWithResultCommandStep1
    {
        private static readonly long DefaultGatewayBrokerTimeoutMillisecond = 20 * 1000;
        private static readonly long DefaultTimeoutAdditionMillisecond = 10 * 1000;

        private readonly CreateWorkflowInstanceWithResultRequest createWithResultRequest;
        private readonly Gateway.GatewayClient client;

        public CreateWorkflowInstanceCommandWithResult(Gateway.GatewayClient client, CreateWorkflowInstanceRequest createRequest)
        {
            this.client = client;
            createWithResultRequest = new CreateWorkflowInstanceWithResultRequest { Request = createRequest };
        }

        /// <inheritdoc/>
        public ICreateWorkflowInstanceWithResultCommandStep1 FetchVariables(IList<string> fetchVariables)
        {
            createWithResultRequest.FetchVariables.AddRange(fetchVariables);
            return this;
        }

        /// <inheritdoc/>
        public ICreateWorkflowInstanceWithResultCommandStep1 FetchVariables(params string[] fetchVariables)
        {
            createWithResultRequest.FetchVariables.AddRange(fetchVariables);
            return this;
        }

        /// <inheritdoc/>
        public async Task<IWorkflowInstanceResult> Send(TimeSpan? timeout = null)
        {
            // this timeout will be used for the Gateway-Broker communication
            createWithResultRequest.RequestTimeout = (long)(timeout?.TotalMilliseconds ?? DefaultGatewayBrokerTimeoutMillisecond);

            // this is the timeout between client and gateway
            var clientDeadline = TimeSpan.FromMilliseconds(createWithResultRequest.RequestTimeout +
                                                           DefaultTimeoutAdditionMillisecond).FromUtcNow();

            var asyncReply = client.CreateWorkflowInstanceWithResultAsync(createWithResultRequest, deadline: clientDeadline);
            var response = await asyncReply.ResponseAsync;
            return new WorkflowInstanceResultResponse(response);
        }

        public async Task<IWorkflowInstanceResult> Send(CancellationToken token)
        {
            var asyncReply = client.CreateWorkflowInstanceWithResultAsync(createWithResultRequest, cancellationToken: token);
            var response = await asyncReply.ResponseAsync;
            return new WorkflowInstanceResultResponse(response);
        }

        public Task<IWorkflowInstanceResult> SendWithRetry(TimeSpan? timespan = null)
        {
            throw new NotImplementedException();
        }
    }
}