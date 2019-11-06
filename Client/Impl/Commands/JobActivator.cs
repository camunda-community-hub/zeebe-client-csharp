using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;
using static GatewayProtocol.Gateway;

namespace Zeebe.Client.Impl.Commands
{
    internal class JobActivator
    {
        private readonly GatewayClient client;

        public JobActivator(GatewayClient client)
        {
            this.client = client;
        }

        public async Task<IActivateJobsResponse> SendActivateRequest(ActivateJobsRequest request, DateTime? requestTimeout = null, CancellationToken? cancelationToken = null)
        {
            DateTime activateRequestTimeout = requestTimeout ?? CalculateRequestTimeout(request);
            using (var stream = client.ActivateJobs(request, deadline: activateRequestTimeout))
            {
                var responseStream = stream.ResponseStream;
                if (await MoveNext(responseStream, cancelationToken))
                {
                    var response = responseStream.Current;
                    return new ActivateJobsResponses(response);
                }

                // empty response
                return new ActivateJobsResponses();
            }
        }

        private static DateTime CalculateRequestTimeout(ActivateJobsRequest request)
        {
            // we need a higher request deadline then the long polling request timeout
            var longPollingTimeout = request.RequestTimeout;
            return longPollingTimeout <= 0
                ? TimeSpan.FromSeconds(10).FromUtcNow()
                : TimeSpan.FromSeconds((longPollingTimeout / 1000f) + 10).FromUtcNow();
        }

        private async Task<bool> MoveNext(IAsyncStreamReader<ActivateJobsResponse> stream, CancellationToken? cancellationToken = null)
        {
            if (cancellationToken.HasValue)
            {
                return await stream.MoveNext(cancellationToken.Value);
            }

            return await stream.MoveNext();
        }
    }
}
