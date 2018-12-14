using System.Collections.Generic;

namespace Zeebe.Client.Api.Responses
{
    /// <summary>
    /// Response for an list workflows request.
    /// </summary>
    public interface IWorkflowListResponse
    {
        /// <returns>
        /// Returns the requested workflow list.
        /// </returns>
        IList<IWorkflowMetadata> WorkflowList { get; }
    }
}