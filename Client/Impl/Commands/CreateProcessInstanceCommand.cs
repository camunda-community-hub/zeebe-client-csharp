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
    public class CreateProcessInstanceCommand
        : ICreateProcessInstanceCommandStep1,
            ICreateProcessInstanceCommandStep2,
            ICreateProcessInstanceCommandStep3
    {
        private const int LatestVersionValue = -1;

        private readonly CreateProcessInstanceRequest request;
        private readonly Gateway.GatewayClient client;

        public CreateProcessInstanceCommand(Gateway.GatewayClient client)
        {
            this.client = client;
            request = new CreateProcessInstanceRequest();
        }

        public ICreateProcessInstanceCommandStep2 BpmnProcessId(string bpmnProcessId)
        {
            request.BpmnProcessId = bpmnProcessId;
            return this;
        }

        public ICreateProcessInstanceCommandStep3 ProcessDefinitionKey(long processDefinitionKey)
        {
            request.ProcessDefinitionKey = processDefinitionKey;
            return this;
        }

        public ICreateProcessInstanceCommandStep3 Version(int version)
        {
            request.Version = version;
            return this;
        }

        public ICreateProcessInstanceCommandStep3 LatestVersion()
        {
            request.Version = LatestVersionValue;
            return this;
        }

        public ICreateProcessInstanceCommandStep3 Variables(string variables)
        {
            request.Variables = variables;
            return this;
        }

        public ICreateProcessInstanceWithResultCommandStep1 WithResult()
        {
            return new CreateProcessInstanceCommandWithResult(client, request);
        }

        public async Task<IProcessInstanceResponse> Send(TimeSpan? timeout = null)
        {
            var asyncReply = client.CreateProcessInstanceAsync(request, deadline: timeout?.FromUtcNow());
            var response = await asyncReply.ResponseAsync;
            return new ProcessInstanceResponse(response);
        }

        public async Task<IProcessInstanceResponse> Send(CancellationToken token)
        {
            var asyncReply = client.CreateProcessInstanceAsync(request, cancellationToken: token);
            var response = await asyncReply.ResponseAsync;
            return new ProcessInstanceResponse(response);
        }
    }
}
