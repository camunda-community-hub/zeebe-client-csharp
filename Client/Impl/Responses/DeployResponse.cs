using System.Collections.Generic;
using System.Linq;
using GatewayProtocol;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class DeployResponse : IDeployResponse
    {
        public long Key { get; }
        public IList<IWorkflowMetadata> Workflows { get; }

        public DeployResponse(DeployWorkflowResponse response)
        {
            Key = response.Key;
            Workflows = response.Workflows
                .Select(metadata => new WorkflowMetadata(metadata))
                .Cast<IWorkflowMetadata>()
                .ToList();
        }
        
    }
}