//
//    Copyright (c) 2018 camunda services GmbH (info@camunda.com)
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using static GatewayProtocol.Gateway;
using PublishMessageResponse = Zeebe.Client.Impl.Responses.PublishMessageResponse;

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

        public IPublishMessageCommandStep3 Variables(string variables)
        {
            request.Variables = variables;
            return this;
        }

        public IPublishMessageCommandStep3 TimeToLive(TimeSpan timeToLive)
        {
            request.TimeToLive = (long)timeToLive.TotalMilliseconds;
            return this;
        }

        public async Task<IPublishMessageResponse> Send(TimeSpan? timeout = null)
        {
            var asyncReply = gatewayClient.PublishMessageAsync(request, deadline: timeout?.FromUtcNow());
            await asyncReply.ResponseAsync;
            return new PublishMessageResponse();
        }
    }
}
