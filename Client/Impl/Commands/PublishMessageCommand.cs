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
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using static GatewayProtocol.Gateway;
using PublishMessageResponse = Zeebe.Client.Impl.Responses.PublishMessageResponse;

namespace Zeebe.Client.Impl.Commands
{
    public class PublishMessageCommand : IPublishMessageCommandStep1, IPublishMessageCommandStep2, IPublishMessageCommandStep3
    {
        private readonly PublishMessageRequest request;
        private readonly GatewayClient gatewayClient;
        private readonly IAsyncRetryStrategy asyncRetryStrategy;

        public PublishMessageCommand(GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy)
        {
            gatewayClient = client;
            request = new PublishMessageRequest();
            this.asyncRetryStrategy = asyncRetryStrategy;
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

        public async Task<IPublishMessageResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
        {
            var asyncReply = gatewayClient.PublishMessageAsync(request, deadline: timeout?.FromUtcNow(), cancellationToken: token);
            await asyncReply.ResponseAsync;
            return new PublishMessageResponse();
        }

        public async Task<IPublishMessageResponse> Send(CancellationToken cancellationToken)
        {
            return await Send(token: cancellationToken);
        }

        public async Task<IPublishMessageResponse> SendWithRetry(TimeSpan? timespan = null, CancellationToken token = default)
        {
            return await asyncRetryStrategy.DoWithRetry(() => Send(timespan, token));
        }
    }
}
