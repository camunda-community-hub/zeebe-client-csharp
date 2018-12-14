using System.Collections.Generic;

namespace Zeebe.Client.Api.Responses
{
    public interface IDeployResponse
    {
        /// <returns>the unique key of the deployment </returns>
        long Key { get; }

        /// <returns>the workflows meta data, which are deployed </returns>
        IList<IWorkflowMetadata> Workflows { get; }
    }
}