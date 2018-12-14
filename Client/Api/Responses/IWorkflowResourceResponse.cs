namespace Zeebe.Client.Api.Responses
{
    public interface IWorkflowResourceResponse
    {
        /// <returns>
        /// the BPMN XML resource of the workflow
        /// </returns>
        string BpmnXml { get; }

        /// <returns>
        /// the version of the deployed workflow
        /// </returns>
        int Version { get; }

        /// <returns>
        /// the bpmn process id
        /// </returns>
        string BpmnProcessId { get; }

        /// <returns>
        /// the resource name
        /// </returns>
        string ResourceName { get; }

        /// <returns>
        /// the unique workflow key
        /// </returns>
        long WorkflowKey { get; }
    }
}