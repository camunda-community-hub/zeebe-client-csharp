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
namespace Zeebe.Client.Api.Responses
{
    public interface IPartitionInfo
    {
        /** @return the partition's id */
        int PartitionId { get; }

        /** @return the current role of the broker for this partition (i.e. leader or follower) */
        PartitionBrokerRole Role { get; }

        /** @return <code>true</code> if the broker is the current leader of this partition */
        bool IsLeader { get; }
    }
}
