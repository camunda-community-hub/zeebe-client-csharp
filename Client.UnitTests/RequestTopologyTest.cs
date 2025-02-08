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

using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client;

[TestFixture]
public class RequestTopologyTest : BaseZeebeTest
{
    [Test]
    public async Task ShouldSendRequestAsExpected()
    {
        // given
        var expectedRequest = new TopologyRequest();

    // when
        _ = await ZeebeClient.TopologyRequest().Send();

        // then
        var actualRequest = TestService.Requests[typeof(TopologyRequest)][0];

        Assert.AreEqual(expectedRequest, actualRequest);
    }

    [Test]
    public void ShouldTimeoutRequest()
    {
        // given

        // when
        var task = ZeebeClient
            .TopologyRequest()
            .Send(TimeSpan.Zero);
        var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
        var rpcException = (RpcException)aggregateException.InnerExceptions[0];

        // then
        Assert.AreEqual(StatusCode.DeadlineExceeded, rpcException.Status.StatusCode);
    }

    [Test]
    public void ShouldCancelRequest()
    {
        // given

        // when
        var task = ZeebeClient
            .TopologyRequest()
            .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
        var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
        var rpcException = (RpcException)aggregateException.InnerExceptions[0];

        // then
        Assert.AreEqual(StatusCode.Cancelled, rpcException.Status.StatusCode);
    }

    [Test]
    public async Task ShouldReceiveResponseAsExpected()
    {
        // given
        var expectedResponse = new TopologyResponse();
        expectedResponse.Brokers.Add(CreateBrokerInfo(0, "host", 26501, 0, true));
        expectedResponse.Brokers.Add(CreateBrokerInfo(1, "host", 26501, 0, false));
        expectedResponse.Brokers.Add(CreateBrokerInfo(2, "host", 26501, 0, false));
        TestService.AddRequestHandler(typeof(TopologyRequest), request => expectedResponse);

        // when
        var response = await ZeebeClient.TopologyRequest().Send();

        // then
        var firstBroker = response.Brokers[0];
        Assert.AreEqual("host0:26501", firstBroker.Address);
        Assert.AreEqual(0, firstBroker.NodeId);

        var firstPartition = firstBroker.Partitions[0];
        Assert.AreEqual(0, firstPartition.PartitionId);
        Assert.AreEqual(PartitionBrokerRole.LEADER, firstPartition.Role);

        var secondBroker = response.Brokers[1];
        Assert.AreEqual("host1:26501", secondBroker.Address);
        Assert.AreEqual(1, secondBroker.NodeId);

        firstPartition = secondBroker.Partitions[0];
        Assert.AreEqual(0, firstPartition.PartitionId);
        Assert.AreEqual(PartitionBrokerRole.FOLLOWER, firstPartition.Role);

        var thirdBroker = response.Brokers[2];
        Assert.AreEqual("host2:26501", thirdBroker.Address);
        Assert.AreEqual(2, thirdBroker.NodeId);

        firstPartition = thirdBroker.Partitions[0];
        Assert.AreEqual(0, firstPartition.PartitionId);
        Assert.AreEqual(PartitionBrokerRole.FOLLOWER, firstPartition.Role);
    }

    private BrokerInfo CreateBrokerInfo(int nodeId, string host, int port, int partitionId, bool leader)
    {
        var brokerInfo = new BrokerInfo
        {
            Host = host + nodeId,
            NodeId = nodeId,
            Port = port
        };

        var partition = new Partition
        {
            PartitionId = partitionId,
            Role = leader
                ? Partition.Types.PartitionBrokerRole.Leader
                : Partition.Types.PartitionBrokerRole.Follower
        };
        brokerInfo.Partitions.Add(partition);

        return brokerInfo;
    }
}