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
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client.Impl.Commands
{
    public class TopologyRequestCommand : ITopologyRequestStep1
    {
        private readonly Gateway.GatewayClient gatewayClient;
        private readonly TopologyRequest request = new TopologyRequest();
        private readonly IAsyncRetryStrategy asyncRetryStrategy;

        public TopologyRequestCommand(Gateway.GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy)
        {
            gatewayClient = client;
            this.asyncRetryStrategy = asyncRetryStrategy;
        }

        public async Task<ITopology> Send(TimeSpan? timeout = null, CancellationToken token = default)
        {
            var asyncReply = gatewayClient.TopologyAsync(request, deadline: timeout?.FromUtcNow(), cancellationToken: token);
            var response = await asyncReply.ResponseAsync;

            return new Topology(response);
        }

        public async Task<ITopology> SendWithRetry(TimeSpan? timespan = null, CancellationToken token = default)
        {
            return await asyncRetryStrategy.DoWithRetry(() => Send(timespan, token));
        }

        public async Task<ITopology> Send(CancellationToken cancellationToken)
        {
            return await Send(token: cancellationToken);
        }
    }
}
