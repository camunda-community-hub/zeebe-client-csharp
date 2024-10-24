using System.Collections.Generic;

namespace Zeebe.Client.Api.Responses;

public interface IDeployResourceResponse
{
    /// <returns>the unique key of the deployment.</returns>
    long Key { get; }

    /// <returns>the processes meta data, which are deployed.</returns>
    IList<IProcessMetadata> Processes { get; }

    /// <returns>the decisions which are deployed.</returns>
    IList<IDecisionMetadata> Decisions { get; }

    /// <returns>the decision requirements which are deployed.</returns>
    IList<IDecisionRequirementsMetadata> DecisionRequirements { get; }

    /// <returns>the forms which are deployed.</returns>
    IList<IFormMetadata> Forms { get; }
}