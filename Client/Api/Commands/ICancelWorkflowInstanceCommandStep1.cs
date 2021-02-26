using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands
{
    public interface ICancelWorkflowInstanceCommandStep1 : IFinalCommandWithRetryStep<ICancelWorkflowInstanceResponse>
    {
        // the place for new optional parameters
    }
}