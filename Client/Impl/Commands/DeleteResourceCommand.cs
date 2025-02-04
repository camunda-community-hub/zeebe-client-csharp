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

namespace Zeebe.Client.Impl.Commands;

internal class DeleteResourceCommand(GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy, long resourceKey)
    : IDeleteResourceCommandStep1
{
    private readonly DeleteResourceRequest request = new()
    {
        ResourceKey = resourceKey
    };

    public async Task<IDeleteResourceResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
    {
        var asyncReply = client.DeleteResourceAsync(request, deadline: timeout?.FromUtcNow(), cancellationToken: token);
        await asyncReply.ResponseAsync;
        return new Responses.DeleteResourceResponse();
    }

    public async Task<IDeleteResourceResponse> Send(CancellationToken cancellationToken)
    {
        return await Send(token: cancellationToken);
    }

    public async Task<IDeleteResourceResponse> SendWithRetry(TimeSpan? timespan = null, CancellationToken token = default)
    {
        return await asyncRetryStrategy.DoWithRetry(() => Send(timespan, token));
    }
}