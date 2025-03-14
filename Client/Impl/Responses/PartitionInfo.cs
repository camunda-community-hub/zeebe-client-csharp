﻿//
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

using GatewayProtocol;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses;

public class PartitionInfo(Partition partition) : IPartitionInfo
{
    public int PartitionId { get; } = partition.PartitionId;
    public bool IsLeader { get; } = partition.Role == Partition.Types.PartitionBrokerRole.Leader;

    public PartitionBrokerRole Role { get; } = partition.Role == Partition.Types.PartitionBrokerRole.Leader
        ? PartitionBrokerRole.LEADER
        : PartitionBrokerRole.FOLLOWER;

    public override string ToString()
    {
        return $"  {nameof(PartitionId)}: {PartitionId}," +
               $"\n  {nameof(IsLeader)}: {IsLeader}," +
               $"\n  {nameof(Role)}: {Role}";
    }
}