using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using ResolveIncidentResponse = Zeebe.Client.Impl.Responses.ResolveIncidentResponse;

namespace Zeebe.Client.Impl.Commands
{
    public class ResolveIncidentCommand : IResolveIncidentCommandStep1
    {
        private readonly ResolveIncidentRequest request;
        private readonly Gateway.GatewayClient client;

        public ResolveIncidentCommand(Gateway.GatewayClient client, long incidentKey)
        {
            request = new ResolveIncidentRequest
            {
                IncidentKey = incidentKey
            };
            this.client = client;
        }

        public async Task<IResolveIncidentResponse> Send(TimeSpan? timeout = null)
        {
            var asyncReply = client.ResolveIncidentAsync(request, deadline: timeout?.FromUtcNow());
            await asyncReply.ResponseAsync;
            return new ResolveIncidentResponse();
        }
    }
}