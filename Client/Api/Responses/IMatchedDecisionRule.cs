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

public interface IMatchedDecisionRule
{
    /// <returns>
    /// the id of the matched rule.
    /// </returns>
    string RuleId { get; }

    /// <returns>
    /// the index of the matched rule.
    /// </returns>
    int RuleIndex { get; }

    /// <returns>
    /// the evaluated decision outputs.
    /// </returns>
    IList<IEvaluatedDecisionOutput> EvaluatedOutputs { get; }
}