using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;
using Zeebe.Client.Helpers;

namespace Zeebe.Client;

[TestFixture]
public class UpdateRetriesTest : BaseZeebeTest
{
    [Test]
    public async Task ShouldSendRequestAsExpected()
    {
        // given
        var expectedRequest = new UpdateJobRetriesRequest
        {
            JobKey = 1024,
            Retries = 223
        };
        TestService.Reset();

        // when
        await ZeebeClient
            .NewUpdateRetriesCommand(1024)
            .Retries(223)
            .Send();

        // then
        var request = TestService.Requests[typeof(UpdateJobRetriesRequest)][0];
        Assert.AreEqual(expectedRequest, request);
    }

    [Test]
    public void ShouldTimeoutRequest()
    {
        // given
        TestService.Reset();

        // when
        var task = ZeebeClient
            .NewUpdateRetriesCommand(1024)
            .Retries(223)
            .Send(TimeSpan.Zero);
        var rpcException = Assert.Throws<RpcException>(() => task.GetAwaiter().GetResult());

        // then
        Assert.AreEqual(StatusCode.DeadlineExceeded, rpcException!.Status.StatusCode);
    }

    [Test]
    public void ShouldCancelRequest()
    {
        // given
        TestService.Reset();

        // when
        var task = ZeebeClient
            .NewUpdateRetriesCommand(1024)
            .Retries(223)
            .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
        var rpcException = Assert.Throws<RpcException>(() => task.GetAwaiter().GetResult());

        // then
        Assert.AreEqual(StatusCode.Cancelled, rpcException!.Status.StatusCode);
    }
}