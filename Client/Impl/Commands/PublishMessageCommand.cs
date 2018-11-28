using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using static GatewayProtocol.Gateway;

namespace Zeebe.Client.Impl.Commands
{
    public class PublishMessageCommand : IPublishMessageCommandStep1, IPublishMessageCommandStep2, IPublishMessageCommandStep3
    {
        private readonly PublishMessageRequest request;
        private readonly GatewayClient gatewayClient;

        public PublishMessageCommand(GatewayClient client)
        {
            gatewayClient = client;
            request = new PublishMessageRequest();
        }

        public IPublishMessageCommandStep3 CorrelationKey(string correlationKey)
        {
            request.CorrelationKey = correlationKey;
            return this;
        }

        public IPublishMessageCommandStep3 MessageId(string messageId)
        {
            request.MessageId = messageId;
            return this;
        }

        public IPublishMessageCommandStep2 MessageName(string messageName)
        {
            request.Name = messageName;
            return this;
        }

        public IPublishMessageCommandStep3 Payload(string payload)
        {
            request.Payload = payload;
            return this;
        }

        public IPublishMessageCommandStep3 TimeToLive(long timeToLive)
        {
            request.TimeToLive = timeToLive;
            return this;
        }

        public Task<Empty> Send()
        {
            gatewayClient.PublishMessage(request);
            return null;
        }
    }
}
