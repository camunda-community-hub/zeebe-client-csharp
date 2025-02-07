using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client;

[TestFixture]
public class CancelProcessInstanceTest : BaseZeebeTest
{
    [Test]
    public async Task ShouldSendRequestAsExpected()
    {
        // given
        var expected = new CancelProcessInstanceRequest
        {
            ProcessInstanceKey = 12113
        };

        // when
        await ZeebeClient.NewCancelInstanceCommand(12113).Send();

        // then
        var request = TestService.Requests[typeof(CancelProcessInstanceRequest)][0];
        Assert.AreEqual(expected, request);
    }

    [Test]
    public void ShouldTimeoutRequest()
    {
        // given

        // when
        var task = ZeebeClient.NewCancelInstanceCommand(12113)
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
        var task = ZeebeClient.NewCancelInstanceCommand(12113)
            .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
        var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
        var rpcException = (RpcException)aggregateException.InnerExceptions[0];

        // then
        Assert.AreEqual(StatusCode.Cancelled, rpcException.Status.StatusCode);
    }
}