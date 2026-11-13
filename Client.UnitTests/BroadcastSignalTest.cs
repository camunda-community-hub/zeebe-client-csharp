using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client;

[TestFixture]
public class BroadcastSignalTest : BaseZeebeTest
{
    [Test]
    public async Task ShouldSendRequestAsExpected()
    {
        // given
        const string variables = "{\"foo\":23}";
        var expectedRequest = new BroadcastSignalRequest
        {
            SignalName = "signalName",
            Variables = variables
        };
        // when
        _ = await ZeebeClient
        .NewBroadcastSignalCommand()
        .SignalName("signalName")
        .Variables(variables)
        .Send();
        // then
        var request = TestService.Requests[typeof(BroadcastSignalRequest)][0];
        Assert.AreEqual(expectedRequest, request);
    }

    [Test]
    public void ShouldTimeoutRequest()
    {
        // given
        // when
        var task = ZeebeClient
            .NewBroadcastSignalCommand()
            .SignalName("signalName")
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
            .NewBroadcastSignalCommand()
            .SignalName("signalName")
            .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
        var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
        var rpcException = (RpcException)aggregateException.InnerExceptions[0];
        // then
        Assert.AreEqual(StatusCode.Cancelled, rpcException.Status.StatusCode);
    }

    [Test]
    public async Task ShouldReceiveResponseWithKeyAndTenantId()
    {
        // given
        const string variables = "{\"foo\":23}";
        const string expectedTenantId = "tenant1";

        // when
        var response = await ZeebeClient
            .NewBroadcastSignalCommand()
            .SignalName("signalName")
            .Variables(variables)
            .AddTenantId(expectedTenantId)
            .Send();

        // then
        Assert.AreEqual(9876543210L, response.Key);
        Assert.AreEqual(expectedTenantId, response.TenantId);
    }

    [Test]
    public async Task ShouldReceiveResponseWithDefaultTenantIdWhenNotSpecified()
    {
        // when
        var response = await ZeebeClient
            .NewBroadcastSignalCommand()
            .SignalName("signalName")
            .Send();

        // then
        Assert.AreEqual(9876543210L, response.Key);
        Assert.AreEqual("<default>", response.TenantId);
    }
}