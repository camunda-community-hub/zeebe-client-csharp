namespace Zeebe.Client.Api.Responses
{
    public interface IWorkflowInstanceResponse
    {
        /// <summary>
        /// Key of the workflow which this instance was created for.
        /// </summary>
        long WorkflowKey { get; }

        /// <summary>
        /// BPMN process id of the workflow which this instance was created for.
        /// </summary>
        string BpmnProcessId { get; }

        /// <summary>
        /// Version of the workflow which this instance was created for.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Unique key of the created workflow instance on the partition.
        /// </summary>
        long WorkflowInstanceKey { get; }
    }
}