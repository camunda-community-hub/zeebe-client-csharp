using System.Collections.Generic;

namespace Zeebe.Client.Api.Responses
{
    public interface IDeployResponse
    {
        /** @return the unique key of the deployment */
        long Key { get; }
        
        
        /** @return the workflows meta data, which are deployed */
        IList<IWorkflowMetadata> Workflows { get; }
    }
}