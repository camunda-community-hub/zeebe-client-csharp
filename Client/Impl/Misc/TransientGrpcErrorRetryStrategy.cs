using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Zeebe.Client.Api.Misc;

namespace Zeebe.Client.Impl.Misc
{
    public class TransientGrpcErrorRetryStrategy : IAsyncRetryStrategy
    {
        private static readonly StatusCode[] RetrieableCodes = { StatusCode.Unavailable, StatusCode.ResourceExhausted };

        private readonly Func<int, TimeSpan> waitTimeProvider;

        public TransientGrpcErrorRetryStrategy(Func<int, TimeSpan> waitTimeProvider)
        {
            this.waitTimeProvider = waitTimeProvider;
        }

        public async Task<TResult> DoWithRetry<TResult>(Func<Task<TResult>> action)
        {
            var retries = 0;
            while (true)
            {
                try
                {
                    var result = await action.Invoke();
                    return result;
                }
                catch (RpcException exception)
                {
                    if (RetrieableCodes.Contains(exception.StatusCode))
                    {
                        var waitTime = waitTimeProvider.Invoke(++retries);
                        await Task.Delay(waitTime);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}