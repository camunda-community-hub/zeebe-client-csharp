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

using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses;

public class DecisionRequirementsMetadata : IDecisionRequirementsMetadata
{
    public string DmnDecisionRequirementsId { get; }
    public string DmnDecisionRequirementsName { get; }
    public int Version { get; }
    public long DecisionRequirementsKey { get; }
    public string ResourceName { get; }

    public DecisionRequirementsMetadata(GatewayProtocol.DecisionRequirementsMetadata metadata)
    {
        DmnDecisionRequirementsId = metadata.DmnDecisionRequirementsId;
        DmnDecisionRequirementsName = metadata.DmnDecisionRequirementsName;
        Version = metadata.Version;
        DecisionRequirementsKey = metadata.DecisionRequirementsKey;
        ResourceName = metadata.ResourceName;
    }
}