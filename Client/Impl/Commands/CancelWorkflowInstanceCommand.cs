using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using CancelWorkflowInstanceResponse = Zeebe.Client.Impl.Responses.CancelWorkflowInstanceResponse;

namespace Zeebe.Client.Impl.Commands
{
    public class CancelWorkflowInstanceCommand : ICancelWorkflowInstanceCommandStep1
    {
        private readonly CancelWorkflowInstanceRequest request;
        private readonly Gateway.GatewayClient client;
        private readonly IAsyncRetryStrategy asyncRetryStrategy;

        public CancelWorkflowInstanceCommand(Gateway.GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy, long workflowInstanceKey)
        {
            request = new CancelWorkflowInstanceRequest
            {
                WorkflowInstanceKey = workflowInstanceKey
            };
            this.client = client;
            this.asyncRetryStrategy = asyncRetryStrategy;
        }

        public async Task<ICancelWorkflowInstanceResponse> Send(TimeSpan? timeout = null)
        {
            var asyncReply = client.CancelWorkflowInstanceAsync(request, deadline: timeout?.FromUtcNow());
            await asyncReply.ResponseAsync;
            return new CancelWorkflowInstanceResponse();
        }

        public async Task<ICancelWorkflowInstanceResponse> SendWithRetry(TimeSpan? timespan = null)
        {
            return await asyncRetryStrategy.DoWithRetry(() => Send(timespan));
        }
    }
}