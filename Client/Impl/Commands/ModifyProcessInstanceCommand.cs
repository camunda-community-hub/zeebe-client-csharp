using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using static GatewayProtocol.Gateway;
using ModifyProcessInstanceResponse = Zeebe.Client.Impl.Responses.ModifyProcessInstanceResponse;

namespace Zeebe.Client.Impl.Commands;

internal class ModifyProcessInstanceCommand(
    GatewayClient client,
    IAsyncRetryStrategy asyncRetryStrategy,
    long processInstanceKey)
    : IModifyProcessInstanceCommandStep1,
        IModifyProcessInstanceCommandStep3
{
    private readonly ModifyProcessInstanceRequest request = new ()
    {
        ProcessInstanceKey = processInstanceKey
    };

    private ModifyProcessInstanceRequest.Types.ActivateInstruction currentActivateInstruction;

    public IModifyProcessInstanceCommandStep3 ActivateElement(string elementId)
    {
        currentActivateInstruction = new ModifyProcessInstanceRequest.Types.ActivateInstruction
        {
            ElementId = elementId,
            AncestorElementInstanceKey = 0
        };
        return this;
    }

    public IModifyProcessInstanceCommandStep3 ActivateElement(string elementId, long ancestorElementInstanceKey)
    {
        currentActivateInstruction = new ModifyProcessInstanceRequest.Types.ActivateInstruction
        {
            ElementId = elementId,
            AncestorElementInstanceKey = ancestorElementInstanceKey
        };
        return this;
    }

    public IModifyProcessInstanceCommandStep2 TerminateElement(long elementInstanceKey)
    {
        request.TerminateInstructions.Add(new ModifyProcessInstanceRequest.Types.TerminateInstruction
        {
            ElementInstanceKey = elementInstanceKey
        });
        return this;
    }

    public IModifyProcessInstanceCommandStep1 And()
    {
        AddCurrentActivateInstruction();
        return this;
    }

    public IModifyProcessInstanceCommandStep3 WithVariables(string variables)
    {
        currentActivateInstruction.VariableInstructions.Add(new ModifyProcessInstanceRequest.Types.VariableInstruction
        {
            Variables = variables
        });
        return this;
    }

    public IModifyProcessInstanceCommandStep3 WithVariables(string variables, string scopeId)
    {
        currentActivateInstruction.VariableInstructions.Add(new ModifyProcessInstanceRequest.Types.VariableInstruction
        {
            Variables = variables,
            ScopeId = scopeId
        });
        return this;
    }

    public async Task<IModifyProcessInstanceResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
    {
        AddCurrentActivateInstruction();

        var asyncReply = client.ModifyProcessInstanceAsync(request, cancellationToken: token);
        _ = await asyncReply.ResponseAsync;
        return new ModifyProcessInstanceResponse();
    }

    public async Task<IModifyProcessInstanceResponse> Send(CancellationToken cancellationToken)
    {
        return await Send(token: cancellationToken);
    }

    public async Task<IModifyProcessInstanceResponse> SendWithRetry(TimeSpan? timeout = null,
        CancellationToken token = default)
    {
        return await asyncRetryStrategy.DoWithRetry(() => Send(timeout, token));
    }

    public IModifyProcessInstanceCommandStep1 AddInstructionToTerminate(long elementInstanceKey)
    {
        request.TerminateInstructions.Add(new ModifyProcessInstanceRequest.Types.TerminateInstruction
        {
            ElementInstanceKey = elementInstanceKey
        });
        return this;
    }

    private void AddCurrentActivateInstruction()
    {
        if (currentActivateInstruction == null)
    {
      return;
    }

        request.ActivateInstructions.Add(currentActivateInstruction);
        currentActivateInstruction = null;
    }
}