using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands;

/// <summary>
/// Command to modify a process instance.
/// </summary>
public interface IModifyProcessInstanceCommandStep1
{
    /// <summary>
    /// Create an activate Instruction
    /// for the given element id. The element will be created within an existing element instance of
    /// the flow scope. When activating an element inside a multi-instance element the element instance
    /// key of the ancestor must be defined. For this use <see cref="ActivateElement(string, long)"/>.
    /// </summary>
    /// <param name="elementId">The id of the element to activate.</param>
    /// <returns>The builder for this command.</returns>
    IModifyProcessInstanceCommandStep3 ActivateElement(string elementId);

    /// <summary>
    /// Create an activate Instruction
    /// for the given element id. The element will be created within the scope that is passed. This
    /// scope must be an ancestor of the element that's getting activated.
    /// </summary>
    /// <param name="elementId">The id of the element to activate.</param>
    /// <param name="ancestorElementInstanceKey">The element instance key in which the element will be created.</param>
    /// <returns>The builder for this command.</returns>
    IModifyProcessInstanceCommandStep3 ActivateElement(string elementId, long ancestorElementInstanceKey);

    /// <summary>
    /// Create a terminate instruction for the given element id.
    /// </summary>
    /// <param name="elementInstanceKey">The element instance key of the element to terminate.</param>
    /// <returns>the builder for this command.</returns>
    IModifyProcessInstanceCommandStep2 TerminateElement(long elementInstanceKey);
}

/// <summary>
/// Second command step, to optional add more instructions to activate or terminate.
/// </summary>
public interface IModifyProcessInstanceCommandStep2 :
    IFinalCommandWithRetryStep<IModifyProcessInstanceResponse>
{
    /// <summary>
    /// Acts as a boundary between the different activate and terminate instructions. Use this if you
    /// want to activate or terminate another element.
    /// Otherwise, use <see cref="IFinalCommandWithRetryStep{T}.SendWithRetry"/> to send the command.
    /// </summary>
    /// <returns>The builder for this command.</returns>
    IModifyProcessInstanceCommandStep1 And();
}

/// <summary>
/// Third command step, to optionally add variables to the element which should be activated.
/// </summary>
public interface IModifyProcessInstanceCommandStep3 : IModifyProcessInstanceCommandStep2
{
    /// <summary>
    ///  Create a variable instruction for the element that's getting activated.
    ///  These variables will be created in the global scope of the process instance.
    /// </summary>
    /// <param name="variables">The variables JSON document as String.</param>
    /// <returns>The builder for this command.</returns>
    IModifyProcessInstanceCommandStep3 WithVariables(string variables);

    /// <summary>
    ///  Create a variable instruction for the element that's getting activated.
    ///  These variables will be created in the scope of the passed element.
    /// </summary>
    /// <param name="variables">The variables JSON document as String.</param>
    /// <param name="scopeId">The id of the element in which scope the variables should be created.</param>
    /// <returns>The builder for this command.</returns>
    IModifyProcessInstanceCommandStep3 WithVariables(string variables, string scopeId);
}