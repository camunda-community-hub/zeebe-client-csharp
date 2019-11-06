using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client.Impl.Commands
{
    public class UpdateRetriesCommand : IUpdateRetriesCommandStep1, IUpdateRetriesCommandStep2
    {
        private readonly UpdateJobRetriesRequest request;
        private readonly Gateway.GatewayClient client;

        public UpdateRetriesCommand(Gateway.GatewayClient client, long jobKey)
        {
            request = new UpdateJobRetriesRequest
            {
                JobKey = jobKey
            };
            this.client = client;
        }

        public IUpdateRetriesCommandStep2 Retries(int retries)
        {
            request.Retries = retries;
            return this;
        }

        public async Task<IUpdateRetriesResponse> Send(TimeSpan? timeout = null)
        {
            var asyncReply = client.UpdateJobRetriesAsync(request, deadline: timeout?.FromUtcNow());
            await asyncReply.ResponseAsync;
            return new UpdateRetriesResponse();
        }
    }
}