using System.Collections.Generic;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands
{
    public interface ICreateWorkflowInstanceCommandStep1
    {
        /// <summary>
        /// Set the BPMN process id of the workflow to create an instance of. This is the static id of the
        /// process in the BPMN XML (i.e. "&#60;bpmn:process id='my-workflow'&#62;").
        /// </summary>
        /// <param name="bpmnProcessId">the BPMN process id of the workflow</param>
        /// <returns>the builder for this command</returns>
        ICreateWorkflowInstanceCommandStep2 BpmnProcessId(string bpmnProcessId);

        /// <summary>
        /// Set the key of the workflow to create an instance of. The key is assigned by the broker while
        /// deploying the workflow. It can be picked from the deployment or workflow event.
        /// </summary>
        /// <param name="workflowKey">the key of the workflow</param>
        /// <returns>the builder for this command</returns>
        ICreateWorkflowInstanceCommandStep3 WorkflowKey(long workflowKey);
    }

    public interface ICreateWorkflowInstanceCommandStep2
    {
        /// <summary>
        /// Set the version of the workflow to create an instance of. The version is assigned by the
        /// broker while deploying the workflow. It can be picked from the deployment or workflow event.
        /// </summary>
        /// <param name="version">the version of the workflow</param>
        /// <returns>the builder for this command</returns>
        ICreateWorkflowInstanceCommandStep3 Version(int version);

        /// <summary>
        /// Use the latest version of the workflow to create an instance of.
        /// <p>
        /// If the latest version was deployed few moments before then it can happen that the new
        /// instance is created of the previous version.
        /// </p>
        /// </summary>
        /// <returns>the builder for this command</returns>
        ICreateWorkflowInstanceCommandStep3 LatestVersion();
    }

    public interface ICreateWorkflowInstanceCommandStep3 : IFinalCommandStep<IWorkflowInstanceResponse>
    {
        /// <summary>
        /// Set the initial variables of the workflow instance.
        /// </summary>
        /// <param name="variables">the variables (JSON) as String</param>
        /// <returns>the builder for this command. Call {@link #send()} to complete the command and send
        ///     it to the broker.</returns>
        ICreateWorkflowInstanceCommandStep3 Variables(string variables);

        /// <summary>
        /// When this method is called, the response to the command will be received after the workflow
        /// is completed. The response consists of a set of variables.
        /// </summary>
        /// <returns>the builder for this command. Call {@link #send()} to complete the command and send it to the broker</returns>
        ICreateWorkflowInstanceWithResultCommandStep1 WithResult();
    }

    public interface ICreateWorkflowInstanceWithResultCommandStep1
        : IFinalCommandStep<IWorkflowInstanceResult>
    {
        /// <summary>
        /// Set a list of variables names which should be fetched in the response.
        /// </summary>
        /// <param name="fetchVariables">set of names of variables to be included in the response</param>
        /// <returns>the builder for this command. Call {@link #send()} to complete the command and send it to the broker</returns>
        ICreateWorkflowInstanceWithResultCommandStep1 FetchVariables(IList<string> fetchVariables);

        /// <summary>
        /// Set a list of variables names which should be fetched in the response.
        /// </summary>
        /// <param name="fetchVariables">set of names of variables to be included in the response</param>
        /// <returns>the builder for this command. Call {@link #send()} to complete the command and send it to the broker</returns>
        ICreateWorkflowInstanceWithResultCommandStep1 FetchVariables(params string[] fetchVariables);
    }
}