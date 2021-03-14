namespace Zeebe.Client.Api.Responses
{
    public interface IWorkflowMetadata
    {
        /// <returns>the BPMN process id of the workflow </returns>
        string BpmnProcessId { get; }

        /// <returns>the version of the deployed workflow </returns>
        int Version { get; }

        /// <summary> <returns>the key of the deployed workflow </returns>
        long WorkflowKey { get; }

        /// <returns>the name of the deployment resource which contains the workflow </returns>
        string ResourceName { get; }
    }
}