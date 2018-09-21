using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GatewayProtocol;

namespace Zeebe.Impl
{
    public class PublishMessageCommand : IPublishMessageCommandStep1, IPublishMessageCommandStep2, IPublishMessageCommandStep3
    {
        private PublishMessageRequest request;
        private GatewayProtocol.Gateway.GatewayClient gatewayClient;

        public PublishMessageCommand(Gateway.GatewayClient client)
        {
            gatewayClient = client;
            request = new PublishMessageRequest();
        }

        public IPublishMessageCommandStep3 CorrelationKey(string correlationKey)
        {
            request.CorrelationKey = correlationKey;
            return this;
        }

        public IPublishMessageCommandStep3 messageId(string messageId)
        {
            request.MessageId = messageId;
            return this;
        }

        public IPublishMessageCommandStep2 MessageName(string messageName)
        {
            request.Name = messageName;
            return this;
        }

        public IPublishMessageCommandStep3 payload(string payload)
        {
            request.Payload = payload;
            return this;
        }

        public IPublishMessageCommandStep3 timeToLive(long timeToLive)
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
