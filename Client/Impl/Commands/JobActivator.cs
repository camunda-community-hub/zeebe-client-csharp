using GatewayProtocol;
using Grpc.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
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
            using (var stream = client.ActivateJobs(request))
            {
                var responseStream = stream.ResponseStream;
                if (await MoveNext(responseStream, cancelationToken))
                {
                    var response = responseStream.Current;
                    return new Responses.ActivateJobsResponses(response);
                }

                // empty response
                return new Responses.ActivateJobsResponses();
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
