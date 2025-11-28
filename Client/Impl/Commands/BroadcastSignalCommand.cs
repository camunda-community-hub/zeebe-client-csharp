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
using BroadcastSignalResponse = Zeebe.Client.Impl.Responses.BroadcastSignalResponse;

namespace Zeebe.Client.Impl.Commands;

public class BroadcastSignalCommand(GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy)
    : IBroadcastSignalCommandStep1, IBroadcastSignalCommandStep2
{
    private readonly BroadcastSignalRequest request = new ();

    public IBroadcastSignalCommandStep2 SignalName(string signalName)
    {
        request.SignalName = signalName;
        return this;
    }

    public IBroadcastSignalCommandStep2 Variables(string variables)
    {
        request.Variables = variables;
        return this;
    }

    public async Task<IBroadcastSignalResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
    {
        var asyncReply =
            client.BroadcastSignalAsync(request, deadline: timeout?.FromUtcNow(), cancellationToken: token);
        _ = await asyncReply.ResponseAsync;
        return new BroadcastSignalResponse();
    }

    public async Task<IBroadcastSignalResponse> Send(CancellationToken cancellationToken)
    {
        return await Send(token: cancellationToken);
    }

    public async Task<IBroadcastSignalResponse> SendWithRetry(TimeSpan? timespan = null,
        CancellationToken token = default)
    {
        return await asyncRetryStrategy.DoWithRetry(() => Send(timespan, token));
    }

    public IBroadcastSignalCommandStep2 AddTenantId(string tenantId)
    {
        request.TenantId = tenantId;
        return this;
    }
}