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

namespace Zeebe.Client.Api.Responses;

public interface IDecisionMetadata
{
    /// <returns>
    /// the dmn decision ID, as parsed during deployment; together with the versions forms a
    /// unique identifier for a specific decision.
    /// </returns>
    string DmnDecisionId { get; }

    /// <returns>the dmn name of the decision, as parsed during deployment.</returns>
    string DmnDecisionName { get; }

    /// <returns>the assigned decision version.</returns>
    int Version { get; }

    /// <returns>the assigned decision key, which acts as a unique identifier for this decision.</returns>
    long DecisionKey { get; }

    /// <returns>
    /// the dmn ID of the decision requirements graph that this decision is part of, as parsed
    /// during deployment.
    /// </returns>
    string DmnDecisionRequirementsId { get; }

    /// <returns>the assigned key of the decision requirements graph that this decision is part of.</returns>
    long DecisionRequirementsKey { get; }
}