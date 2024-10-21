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

using System.Collections.Generic;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses;

public class EvaluatedDecisionResponse : IEvaluateDecisionResponse
{
    public string DecisionId { get; }
    public int DecisionVersion { get; }
    public long DecisionKey { get; }
    public string DecisionName { get; }
    public string DecisionRequirementsId { get; }
    public long DecisionRequirementsKey { get; }
    public string DecisionOutput { get; }
    public IList<IEvaluatedDecision> EvaluatedDecisions { get; }
    public string FailedDecisionId { get; }
    public string FailureMessage { get; }

    public EvaluatedDecisionResponse(GatewayProtocol.EvaluateDecisionResponse response)
    {
        DecisionId = response.DecisionId;
        DecisionVersion = response.DecisionVersion;
        DecisionKey = response.DecisionKey;
        DecisionName = response.DecisionName;
        DecisionRequirementsId = response.DecisionRequirementsId;
        DecisionRequirementsKey = response.DecisionRequirementsKey;
        DecisionOutput = response.DecisionOutput;
        EvaluatedDecisions = new List<IEvaluatedDecision>();
        foreach (var decision in response.EvaluatedDecisions)
        {
            EvaluatedDecisions.Add(new EvaluatedDecision(decision));
        }

        FailedDecisionId = response.FailedDecisionId;
        FailureMessage = response.FailureMessage;
    }
}