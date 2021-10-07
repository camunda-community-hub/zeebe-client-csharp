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

namespace Zeebe.Client.Impl.Commands
{
    public class ThrowErrorCommand : IThrowErrorCommandStep1, IThrowErrorCommandStep2
    {
        private readonly ThrowErrorRequest request;
        private readonly GatewayClient gatewayClient;
        private readonly IAsyncRetryStrategy asyncRetryStrategy;

        public ThrowErrorCommand(GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy, long jobKey)
        {
            gatewayClient = client;
            this.asyncRetryStrategy = asyncRetryStrategy;
            request = new ThrowErrorRequest
            {
                JobKey = jobKey
            };
        }

        public IThrowErrorCommandStep2 ErrorCode(string errorCode)
        {
            request.ErrorCode = errorCode;
            return this;
        }

        public IThrowErrorCommandStep2 ErrorMessage(string errorMessage)
        {
            request.ErrorMessage = errorMessage;
            return this;
        }

        public async Task<IThrowErrorResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
        {
            var asyncReply = gatewayClient.ThrowErrorAsync(request, deadline: timeout?.FromUtcNow(), cancellationToken: token);
            await asyncReply.ResponseAsync;
            return new Responses.ThrowErrorResponse();
        }

        public async Task<IThrowErrorResponse> Send(CancellationToken cancellationToken)
        {
            return await Send(token: cancellationToken);
        }

        public async Task<IThrowErrorResponse> SendWithRetry(TimeSpan? timespan = null, CancellationToken token = default)
        {
            return await asyncRetryStrategy.DoWithRetry(() => Send(timespan, token));
        }
    }
}
