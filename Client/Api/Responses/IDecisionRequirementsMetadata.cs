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

public interface IDecisionRequirementsMetadata
{
    /// <returns>
    ///     the dmn decision requirements ID, as parsed during deployment; together with the
    ///     versions forms a unique identifier for a specific decision.
    /// </returns>
    string DmnDecisionRequirementsId { get; }

    /// <returns>the dmn name of the decision requirements, as parsed during deployment.</returns>
    string DmnDecisionRequirementsName { get; }

    /// <returns>the assigned decision requirements version.</returns>
    int Version { get; }

    /// <returns>
    ///     the assigned decision requirements key, which acts as a unique identifier for this
    ///     decision requirements.
    /// </returns>
    long DecisionRequirementsKey { get; }

    /// <returns>
    ///     the resource name (i.e. filename) from which this decision requirements was parsed.
    /// </returns>
    string ResourceName { get; }
}