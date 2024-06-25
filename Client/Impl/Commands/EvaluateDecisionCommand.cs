//
//     Copyright (c) 2021 camunda services GmbH (info@camunda.com)
//
//     Licensed under the Apache License, Version 2.0 (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//
//         http://www.apache.org/licenses/LICENSE-2.0
//
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client.Impl.Commands;

public class EvaluateDecisionCommand : IEvaluateDecisionCommandStep1, IEvaluateDecisionCommandStep1.IEvaluateDecisionCommandStep2
{
    private readonly EvaluateDecisionRequest request;
    private readonly Gateway.GatewayClient gatewayClient;
    private readonly IAsyncRetryStrategy asyncRetryStrategy;

    public EvaluateDecisionCommand(Gateway.GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy)
    {
        gatewayClient = client;
        request = new EvaluateDecisionRequest();
        this.asyncRetryStrategy = asyncRetryStrategy;
    }

    public IEvaluateDecisionCommandStep1.IEvaluateDecisionCommandStep2 DecisionId(string decisionId)
    {
        request.DecisionId = decisionId;
        return this;
    }

    public IEvaluateDecisionCommandStep1.IEvaluateDecisionCommandStep2 DecisionKey(long decisionKey)
    {
        request.DecisionKey = decisionKey;
        return this;
    }

    public IEvaluateDecisionCommandStep1.IEvaluateDecisionCommandStep2 AddTenantId(string tenantId)
    {
        request.TenantId = tenantId;
        return this;
    }

    public async Task<IEvaluateDecisionResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
    {
        var asyncReply = gatewayClient.EvaluateDecisionAsync(request, deadline: timeout?.FromUtcNow(), cancellationToken: token);
        var response = await asyncReply.ResponseAsync;
        return new EvaluatedDecisionResponse(response);
    }

    public async Task<IEvaluateDecisionResponse> Send(CancellationToken cancellationToken)
    {
        return await Send(token: cancellationToken);
    }

    public async Task<IEvaluateDecisionResponse> SendWithRetry(TimeSpan? timespan = null, CancellationToken token = default)
    {
        return await asyncRetryStrategy.DoWithRetry(() => Send(timespan, token));
    }

    public IEvaluateDecisionCommandStep1.IEvaluateDecisionCommandStep2 Variables(string variables)
    {
        request.Variables = variables;
        return this;
    }
}