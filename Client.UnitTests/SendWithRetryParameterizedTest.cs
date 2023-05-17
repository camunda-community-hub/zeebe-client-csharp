using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using NUnit.Framework;
using Zeebe.Client.Helpers;

namespace Zeebe.Client;

[TestFixture]
public class SendWithRetryParameterizedTest : BaseZeebeTest
{
    [Test]
    [TestCaseSource(typeof(TestDataProvider), "Provider")]
    public async Task ShouldRetrySendRequestOnResourceExhaustedUntilSucceed<TReq, TRes, TClientResponse>(
        TReq expectedRequest,
        TRes response,
        RequestCreator<TClientResponse> requestCreator) where TRes : IMessage
    {
        // given
        var countdownEvent = new CountdownEvent(5);
        TestService.AddRequestHandler<TReq>(
            _ =>
            {
                countdownEvent.Signal();
                throw new RpcException(new Status(StatusCode.ResourceExhausted, "exhausted"));
            });

        // when
        var resultTask = requestCreator(ZeebeClient).SendWithRetry();
        countdownEvent.Wait(TimeSpan.FromSeconds(10));

        // then
        Assert.AreEqual(0, countdownEvent.CurrentCount);
        TestService.AddRequestHandler<TReq>(_ => response, false);
        await resultTask;

        var request = TestService.Requests[typeof(TReq)][0];
        Assert.AreEqual(expectedRequest, request);

        var requestCount = TestService.Requests[typeof(TReq)].Count;
        Assert.GreaterOrEqual(requestCount, 5);
    }

    [Test]
    [TestCaseSource(typeof(TestDataProvider), "Provider")]
    public async Task ShouldRetrySendRequestOnUnavailableUntilSucceed<TReq, TRes, TClientResponse>(
        TReq expectedRequest,
        TRes response,
        RequestCreator<TClientResponse> requestCreator) where TRes : IMessage
    {
        // given
        var countdownEvent = new CountdownEvent(5);
        TestService.AddRequestHandler<TReq>(_ =>
        {
            countdownEvent.Signal();
            throw new RpcException(new Status(StatusCode.Unavailable, "exhausted"));
        });

        // when
        var resultTask = requestCreator(ZeebeClient).SendWithRetry();
        countdownEvent.Wait(TimeSpan.FromSeconds(10));

        // then
        Assert.AreEqual(0, countdownEvent.CurrentCount);
        TestService.AddRequestHandler<TReq>(_ => response, false);
        await resultTask;

        var request = TestService.Requests[typeof(TReq)][0];
        Assert.AreEqual(expectedRequest, request);

        var requestCount = TestService.Requests[typeof(TReq)].Count;
        Assert.GreaterOrEqual(requestCount, 5);
    }

    [Test]
    [TestCaseSource(typeof(TestDataProvider), "Provider")]
    public void ShouldNotRetrySendRequest<TReq, TRes, TClientResponse>(
        TReq expectedRequest,
        TRes response,
        RequestCreator<TClientResponse> requestCreator) where TRes : IMessage
    {
        // given
        TestService.AddRequestHandler<TReq>(_ => throw new RpcException(new Status(StatusCode.Internal, "exhausted")));

        // when
        var resultTask = requestCreator(ZeebeClient).SendWithRetry();
        var rpcException = Assert.Throws<RpcException>(() => resultTask.GetAwaiter().GetResult());

        // then
        Assert.AreEqual(StatusCode.Internal, rpcException!.Status.StatusCode);
    }
}