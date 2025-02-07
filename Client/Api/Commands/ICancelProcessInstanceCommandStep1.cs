using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands;

public interface ICancelProcessInstanceCommandStep1 : IFinalCommandWithRetryStep<ICancelProcessInstanceResponse>
{
    // the place for new optional parameters
}