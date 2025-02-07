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

public class EvaluatedDecision : IEvaluatedDecision
{
    public EvaluatedDecision(GatewayProtocol.EvaluatedDecision evaluatedDecision)
    {
        DecisionId = evaluatedDecision.DecisionId;
        DecisionVersion = evaluatedDecision.DecisionVersion;
        DecisionKey = evaluatedDecision.DecisionKey;
        DecisionName = evaluatedDecision.DecisionName;
        DecisionType = evaluatedDecision.DecisionType;
        DecisionOutput = evaluatedDecision.DecisionOutput;

        EvaluatedInputs = new List<IEvaluatedDecisionInput>();
        foreach (var input in evaluatedDecision.EvaluatedInputs) EvaluatedInputs.Add(new EvaluatedDecisionInput(input));

        MatchedRules = new List<IMatchedDecisionRule>();
        foreach (var matchedRule in evaluatedDecision.MatchedRules)
            MatchedRules.Add(new MatchedDecisionRule(matchedRule));
    }

    public string DecisionId { get; }
    public int DecisionVersion { get; }
    public long DecisionKey { get; }
    public string DecisionName { get; }
    public string DecisionType { get; }
    public string DecisionOutput { get; }
    public IList<IEvaluatedDecisionInput> EvaluatedInputs { get; }
    public IList<IMatchedDecisionRule> MatchedRules { get; }
}