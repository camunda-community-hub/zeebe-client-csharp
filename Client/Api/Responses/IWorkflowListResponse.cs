using System.Collections.Generic;

namespace Zeebe.Client.Api.Responses
{
    public interface IWorkflowListResponse
    {
        IList<IWorkflowMetadata> WorkflowList { get; }
    }
}