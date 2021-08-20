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
    public delegate Task ConsumeJob(IActivateJobsResponse response);
    internal class JobActivator
    {
        private readonly GatewayClient client;

        public JobActivator(GatewayClient client)
        {
            this.client = client;
        }

        public async Task SendActivateRequest(ActivateJobsRequest request, ConsumeJob consumer, DateTime? requestTimeout = null, CancellationToken? cancellationToken = null)
        {
            var activateRequestTimeout = requestTimeout ?? CalculateRequestTimeout(request);
            using (var stream = client.ActivateJobs(request, deadline: activateRequestTimeout))
            {
                var responseStream = stream.ResponseStream;

                while (await MoveNext(responseStream, cancellationToken))
                {
                    var currentResponse = responseStream.Current;
                    var response = new ActivateJobsResponses(currentResponse);
                    await consumer.Invoke(response);
                }
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

        private static async Task<bool> MoveNext(IAsyncStreamReader<ActivateJobsResponse> stream, CancellationToken? cancellationToken = null)
        {
            if (cancellationToken.HasValue)
            {
                return await stream.MoveNext(cancellationToken.Value);
            }

            return await stream.MoveNext();
        }
    }
}
