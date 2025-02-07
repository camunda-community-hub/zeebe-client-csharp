using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands;

public interface IResolveIncidentCommandStep1 : IFinalCommandWithRetryStep<IResolveIncidentResponse>
{
    // the place for new optional parameters
}