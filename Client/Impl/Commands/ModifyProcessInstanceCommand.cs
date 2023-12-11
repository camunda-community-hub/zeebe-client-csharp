using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;

using static GatewayProtocol.Gateway;

namespace Zeebe.Client.Impl.Commands;

internal class ModifyProcessInstanceCommand : IModifyProcessInstanceCommandStep1
{
    private readonly ModifyProcessInstanceRequest request;
    private readonly GatewayClient client;
    private readonly IAsyncRetryStrategy asyncRetryStrategy;

    private readonly IList<ModifyProcessInstanceRequest.Types.TerminateInstruction> terminateInstructions;
    private readonly IList<ModifyProcessInstanceRequest.Types.ActivateInstruction> activateInstructions;

    public ModifyProcessInstanceCommand(GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy, long processInstanceKey)
    {
        terminateInstructions = new List<ModifyProcessInstanceRequest.Types.TerminateInstruction>();
        activateInstructions = new List<ModifyProcessInstanceRequest.Types.ActivateInstruction>();

        this.asyncRetryStrategy = asyncRetryStrategy;
        this.client = client;

        request = new ModifyProcessInstanceRequest
        {
            ProcessInstanceKey = processInstanceKey
        };
    }

    public IModifyProcessInstanceCommandStep1 AddInstructionToTerminate(long elementInstanceKey)
    {
        var terminateInstruction = new ModifyProcessInstanceRequest.Types.TerminateInstruction
        {
            ElementInstanceKey = elementInstanceKey
        };

        terminateInstructions.Add(terminateInstruction);
        return this;
    }

    public IModifyProcessInstanceCommandStep1 AddInstructionToActivate(
        string elementId,
        long ancestorElementInstanceKey = 0)
    {
        var activateInstruction = new ModifyProcessInstanceRequest.Types.ActivateInstruction
        {
            ElementId = elementId,
            AncestorElementInstanceKey = ancestorElementInstanceKey
        };

        activateInstructions.Add(activateInstruction);
        return this;
    }

    public async Task<IModifyProcessInstanceResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
    {
        request.TerminateInstructions.AddRange(terminateInstructions);
        request.ActivateInstructions.AddRange(activateInstructions);

        var asyncReply = client.ModifyProcessInstanceAsync(request, cancellationToken: token);
        await asyncReply.ResponseAsync;
        return new Responses.ModifyProcessInstanceResponse();
    }

    public async Task<IModifyProcessInstanceResponse> Send(CancellationToken cancellationToken)
    {
        return await Send(token: cancellationToken);
    }

    public async Task<IModifyProcessInstanceResponse> SendWithRetry(TimeSpan? timeout = null, CancellationToken token = default)
    {
        return await asyncRetryStrategy.DoWithRetry(() => Send(timeout, token));
    }
}