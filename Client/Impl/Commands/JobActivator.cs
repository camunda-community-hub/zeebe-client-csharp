using GatewayProtocol;
using System.Collections.Generic;
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

        public async Task<IActivateJobsResponse> SendActivateRequest(ActivateJobsRequest request)
        {
            using (var stream = client.ActivateJobs(request))
            {
                var responseStream = stream.ResponseStream;
                if (await responseStream.MoveNext())
                {
                    var response = responseStream.Current;
                    return new Responses.ActivateJobsResponses(response);
                }
                else
                {
                    // empty response
                    return new Responses.ActivateJobsResponses();
                }
            }
        }
    }
}
