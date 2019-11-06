using System;
using System.Collections.Generic;
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
            var asyncReply = client.CreateWorkflowInstanceWithResultAsync(createWithResultRequest, deadline: timeout?.FromUtcNow());
            var response = await asyncReply.ResponseAsync;
            return new WorkflowInstanceResultResponse(response);
        }
    }
}