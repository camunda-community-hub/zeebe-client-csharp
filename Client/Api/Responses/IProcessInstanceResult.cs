namespace Zeebe.Client.Api.Responses
{
    /// <summary>
    /// Response for an create workflow instance with result command.
    /// </summary>
    public interface IWorkflowInstanceResult
    {
        /// <returns>
        /// Key of the workflow which this instance was created for.
        /// </returns>
        long WorkflowKey { get; }

        /// <returns>
        /// BPMN process id of the workflow which this instance was created for.
        /// </returns>
        string BpmnProcessId { get; }

        /// <returns>
        /// Version of the workflow which this instance was created for.
        /// </returns>
        int Version { get; }

        /// <returns>
        /// Unique key of the created workflow instance on the partition.
        /// </returns>
        long WorkflowInstanceKey { get; }

        /// <returns> JSON-formatted variables </returns>
        string Variables { get; }
    }
}