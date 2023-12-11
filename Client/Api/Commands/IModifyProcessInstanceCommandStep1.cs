﻿using System.Collections.Generic;
using GatewayProtocol;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands;

public interface IModifyProcessInstanceCommandStep1 :
    IFinalCommandWithRetryStep<IModifyProcessInstanceResponse>
{
    /// <summary>
    /// Set the collection of instructions to terminate at defined process instance.
    /// </summary>
    /// <param name="elementInstanceKey">Element instance key for single terminate instruction.</param>
    /// <returns>the builder for this command.</returns>
    IModifyProcessInstanceCommandStep1 AddInstructionToTerminate(
        long elementInstanceKey);

    /// <summary>
    /// Set the collection of instructions to activate at defined process instance.
    /// </summary>
    /// <param name="elementId">Element id for single instruction to activate.</param>
    /// <param name="ancestorElementInstanceKey">Element key of ancestor to define scope to activate instruction.</param>
    /// <returns>the builder for this command.</returns>
    IModifyProcessInstanceCommandStep1 AddInstructionToActivate(string elementId, long ancestorElementInstanceKey = 0);
}