using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands
{
    public interface IDeleteResourceCommandStep1 : IFinalCommandWithRetryStep<IDeleteResourceResponse>;
}