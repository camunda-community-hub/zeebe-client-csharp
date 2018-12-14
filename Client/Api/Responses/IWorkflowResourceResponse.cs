using System.IO;

namespace Zeebe.Client.Api.Responses
{
    public interface IWorkflowResourceResponse
    {
        /// <summary>
        /// the BPMN XML resource of the workflow
        /// </summary>
        string BpmnXml { get; }

        /// <summary>
        /// the version of the deployed workflow
        /// </summary>
        int Version { get; }

        /// <summary>
        /// the bpmn process id
        /// </summary>
        string BpmnProcessId { get; }

        /// <summary>
        /// the resource name
        /// </summary>
        string ResourceName { get; }

        /// <summary>
        /// the unique workflow key
        /// </summary>
        long WorkflowKey { get; }
    }
}