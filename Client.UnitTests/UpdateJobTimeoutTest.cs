using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client;

[TestFixture]
public class UpdateJobTimeoutTest : BaseZeebeTest
{
    [Test]
    public async Task ShouldSendRequestAsExpected()
    {
        // given
        var expectedRequest = new UpdateJobTimeoutRequest
        {
            JobKey = 1024,
            Timeout = 2000
        };

        // when
        await ZeebeClient
            .NewUpdateJobTimeoutCommand(1024)
            .Timeout(new TimeSpan(0, 0, 2))
            .Send();

        // then
        var request = TestService.Requests[typeof(UpdateJobTimeoutRequest)][0];
        Assert.AreEqual(expectedRequest, request);
    }

    [Test]
    public void ShouldTimeoutRequest()
    {
        // given

        // when
        var task = ZeebeClient
            .NewUpdateJobTimeoutCommand(1024)
            .Timeout(new TimeSpan(0, 0, 2))
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
            .NewUpdateJobTimeoutCommand(1024)
            .Timeout(new TimeSpan(0, 0, 2))
            .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
        var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
        var rpcException = (RpcException)aggregateException.InnerExceptions[0];

        // then
        Assert.AreEqual(StatusCode.Cancelled, rpcException.Status.StatusCode);
    }
}