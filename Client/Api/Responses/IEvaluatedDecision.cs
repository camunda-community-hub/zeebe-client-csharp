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

namespace Zeebe.Client.Api.Responses;

public interface IEvaluatedDecision
{
    /// <returns>
    /// the decision ID, as parsed during deployment; together with the versions forms a unique
    /// identifier for a specific decision.
    /// </returns>
    string DecisionId { get; }

    /// <returns>
    /// the assigned decision version.
    /// </returns>
    int DecisionVersion { get; }

    /// <returns>
    /// the assigned decision key, which acts as a unique identifier for this decision.
    /// </returns>
    long DecisionKey { get; }

    /// <returns>
    /// the name of the decision, as parsed during deployment.
    /// </returns>
    string DecisionName { get; }

    /// <returns>
    /// the type of the evaluated decision.
    /// </returns>
    string DecisionType { get; }

    /// <returns>
    /// the output of the evaluated decision.
    /// </returns>
    string DecisionOutput { get; }

    /// <returns>
    /// the decision inputs that were evaluated within this decision evaluation.
    /// </returns>
    IList<IEvaluatedDecisionInput> EvaluatedInputs { get; }

    /// <returns>
    /// the decision rules that matched within this decision evaluation.
    /// </returns>
    IList<IMatchedDecisionRule> MatchedRules { get; }
}