using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client.Impl.Commands;

public class CreateProcessInstanceCommand(Gateway.GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy)
    : ICreateProcessInstanceCommandStep1,
        ICreateProcessInstanceCommandStep2,
        ICreateProcessInstanceCommandStep3
{
    private const int LatestVersionValue = -1;

    private readonly CreateProcessInstanceRequest request = new ();

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

    public ICreateProcessInstanceCommandStep3 AddTenantId(string tenantId)
    {
        request.TenantId = tenantId;
        return this;
    }

    public ICreateProcessInstanceCommandStep3 AddStartInstruction(string elementId)
    {
        request.StartInstructions.Add(new ProcessInstanceCreationStartInstruction { ElementId = elementId });
        return this;
    }

    public ICreateProcessInstanceWithResultCommandStep1 WithResult()
    {
        return new CreateProcessInstanceCommandWithResult(client, asyncRetryStrategy, request);
    }

    public async Task<IProcessInstanceResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
    {
        var asyncReply = client.CreateProcessInstanceAsync(request, deadline: timeout?.FromUtcNow(), cancellationToken: token);
        var response = await asyncReply.ResponseAsync;
        return new ProcessInstanceResponse(response);
    }

    public async Task<IProcessInstanceResponse> Send(CancellationToken cancellationToken)
    {
        return await Send(token: cancellationToken);
    }

    public async Task<IProcessInstanceResponse> SendWithRetry(TimeSpan? timespan = null, CancellationToken token = default)
    {
        return await asyncRetryStrategy.DoWithRetry(() => Send(timespan, token));
    }
}