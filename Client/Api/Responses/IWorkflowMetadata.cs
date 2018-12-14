namespace Zeebe.Client.Api.Responses
{
    public interface IWorkflowMetadata
    {
        /** @return the BPMN process id of the workflow */
        string BpmnProcessId { get; }

        /** @return the version of the deployed workflow */
        int Version { get; }

        /** @return the key of the deployed workflow */
        long WorkflowKey { get; }

        /** @return the name of the deployment resource which contains the workflow */
        string ResourceName { get; }
    }
}