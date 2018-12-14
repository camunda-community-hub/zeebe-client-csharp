using System.Collections.Generic;
using System.Linq;
using GatewayProtocol;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class WorkflowListResponse : IWorkflowListResponse
    {
        public IList<IWorkflowMetadata> WorkflowList { get; }
        
        public WorkflowListResponse(ListWorkflowsResponse response)
        {
            WorkflowList = response.Workflows.Select(metadata => new WorkflowMetadata(metadata)).Cast<IWorkflowMetadata>().ToList();
        }


    }
}