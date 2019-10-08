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

        public async Task<IActivateJobsResponse> SendActivateRequest(ActivateJobsRequest request, CancellationToken? cancelationToken = null)
        {
            // we need a higher request deadline then the long polling request timeout
            var requestTimeout = request.RequestTimeout <= 0 ? 10 : (request.RequestTimeout / 1000) + 10;
            using (var stream = client.ActivateJobs(request, deadline: DateTime.UtcNow.AddSeconds(requestTimeout)))
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
