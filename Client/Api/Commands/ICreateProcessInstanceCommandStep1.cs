using System.Collections.Generic;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands;

public interface ICreateProcessInstanceCommandStep1
{
    /// <summary>
    /// Set the BPMN process id of the process to create an instance of. This is the static id of the
    /// process in the BPMN XML (i.e. "&#60;bpmn:process id='my-process'&#62;").
    /// </summary>
    /// <param name="bpmnProcessId">the BPMN process id of the process.</param>
    /// <returns>the builder for this command.</returns>
    ICreateProcessInstanceCommandStep2 BpmnProcessId(string bpmnProcessId);

    /// <summary>
    /// Set the key of the process to create an instance of. The key is assigned by the broker while
    /// deploying the process. It can be picked from the deployment or process event.
    /// </summary>
    /// <param name="processDefinitionKey">the key of the process.</param>
    /// <returns>the builder for this command.</returns>
    ICreateProcessInstanceCommandStep3 ProcessDefinitionKey(long processDefinitionKey);
}

public interface ICreateProcessInstanceCommandStep2
{
    /// <summary>
    /// Set the version of the process to create an instance of. The version is assigned by the
    /// broker while deploying the process. It can be picked from the deployment or process event.
    /// </summary>
    /// <param name="version">the version of the process.</param>
    /// <returns>the builder for this command.</returns>
    ICreateProcessInstanceCommandStep3 Version(int version);

    /// <summary>
    /// Use the latest version of the process to create an instance of.
    /// <para>
    /// If the latest version was deployed few moments before then it can happen that the new
    /// instance is created of the previous version.
    /// </para>
    /// </summary>
    /// <returns>the builder for this command.</returns>
    ICreateProcessInstanceCommandStep3 LatestVersion();
}

public interface ICreateProcessInstanceCommandStep3 : IFinalCommandWithRetryStep<IProcessInstanceResponse>, 
    ITenantIdCommandStep<ICreateProcessInstanceCommandStep3>
{
    /// <summary>
    /// Set the initial variables of the process instance.
    /// </summary>
    /// <param name="variables">the variables (JSON) as String.</param>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/>
    /// to complete the command and send it to the broker.</returns>
    ICreateProcessInstanceCommandStep3 Variables(string variables);

    /// <summary>
    /// When this method is called, the response to the command will be received after the process
    /// is completed. The response consists of a set of variables.
    /// </summary>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.</returns>
    ICreateProcessInstanceWithResultCommandStep1 WithResult();
}

public interface ICreateProcessInstanceWithResultCommandStep1
    : IFinalCommandWithRetryStep<IProcessInstanceResult>
{
    /// <summary>
    /// Set a list of variables names which should be fetched in the response.
    /// </summary>
    /// <param name="fetchVariables">set of names of variables to be included in the response.</param>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.</returns>
    ICreateProcessInstanceWithResultCommandStep1 FetchVariables(IList<string> fetchVariables);

    /// <summary>
    /// Set a list of variables names which should be fetched in the response.
    /// </summary>
    /// <param name="fetchVariables">set of names of variables to be included in the response.</param>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.</returns>
    ICreateProcessInstanceWithResultCommandStep1 FetchVariables(params string[] fetchVariables);
}