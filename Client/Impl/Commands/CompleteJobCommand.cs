using GatewayProtocol;
using System.Threading.Tasks;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using static GatewayProtocol.Gateway;

namespace Zeebe.Client.Impl.Commands
{
    class CompleteJobCommand : ICompleteJobCommandStep1
    {
        private readonly CompleteJobRequest request;
        private readonly GatewayClient gatewayClient;

        public CompleteJobCommand(GatewayClient client, long jobKey)
        {
            gatewayClient = client;
            request = new CompleteJobRequest
            {
                JobKey = jobKey
            };
        }

        public ICompleteJobCommandStep1 Payload(string payload)
        {
            request.Payload = payload;
            return this;
        }

        public async Task<ICompleteJobResponse> Send()
        {
            var asyncReply = gatewayClient.CompleteJobAsync(request);
            await asyncReply.ResponseAsync;
            return new Responses.CompleteJobResponse();
        }
    }
}
