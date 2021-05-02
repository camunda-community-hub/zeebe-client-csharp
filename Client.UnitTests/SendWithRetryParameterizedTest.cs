using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Google.Protobuf;
using Grpc.Core;
using NLog.Filters;
using NUnit.Framework;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client
{
    public delegate IFinalCommandWithRetryStep<T> RequestCreator<T>(IZeebeClient zeebeClient);

    public class TestDataProvider
    {
        public static IEnumerable<TestCaseData> Provider()
        {
            yield return new TestCaseData(
                new CancelProcessInstanceRequest
            {
                ProcessInstanceKey = 12113
            }, new CancelProcessInstanceResponse(),
                (RequestCreator<ICancelProcessInstanceResponse>)
                (zeebeClient => zeebeClient.NewCancelInstanceCommand(12113)));
        }
    }

    [TestFixture]
    public class SendWithRetryParameterizedTest : BaseZeebeTest
    {
        [Test, TestCaseSource(typeof(TestDataProvider), "Provider")]
        public async Task ShouldRetrySendRequestOnResourceExhaustedUntilSucceed<TReq, TRes, TClientResponse>(
            TReq expectedRequest,
            TRes response,
            RequestCreator<TClientResponse> requestCreator) where TRes : IMessage
        {
            // given
            var countdownEvent = new CountdownEvent(5);
            TestService.AddRequestHandler(typeof(TReq),
                req =>
                {
                    countdownEvent.Signal();
                    throw new RpcException(new Status(StatusCode.ResourceExhausted, "exhausted"));
                });

            // when
            var resultTask = requestCreator(ZeebeClient).SendWithRetry();
            countdownEvent.Wait(TimeSpan.FromSeconds(10));

            // then
            Assert.AreEqual(0, countdownEvent.CurrentCount);
            TestService.AddRequestHandler(typeof(TReq), req => response);
            await resultTask;

            var request = TestService.Requests[typeof(TReq)][0];
            Assert.AreEqual(expectedRequest, request);

            var requestCount = TestService.Requests[typeof(TReq)].Count;
            Assert.GreaterOrEqual(requestCount, 5);
        }

        [Test, TestCaseSource(typeof(TestDataProvider), "Provider")]
        public async Task ShouldRetrySendRequestOnUnavailableUntilSucceed<TReq, TRes, TClientResponse>(
            TReq expectedRequest,
            TRes response,
            RequestCreator<TClientResponse> requestCreator) where TRes : IMessage
        {
            // given
            var countdownEvent = new CountdownEvent(5);
            TestService.AddRequestHandler(
                typeof(TReq),
                req =>
                {
                    countdownEvent.Signal();
                    throw new RpcException(new Status(StatusCode.Unavailable, "exhausted"));
                });

            // when
            var resultTask = requestCreator(ZeebeClient).SendWithRetry();
            countdownEvent.Wait(TimeSpan.FromSeconds(10));

            // then
            Assert.AreEqual(0, countdownEvent.CurrentCount);
            TestService.AddRequestHandler(typeof(TReq), req => response);
            await resultTask;

            var request = TestService.Requests[typeof(TReq)][0];
            Assert.AreEqual(expectedRequest, request);

            var requestCount = TestService.Requests[typeof(TReq)].Count;
            Assert.GreaterOrEqual(requestCount, 5);
        }

        [Test, TestCaseSource(typeof(TestDataProvider), "Provider")]
        public void ShouldNotRetrySendRequest<TReq, TRes, TClientResponse>(
            TReq expectedRequest,
            TRes response,
            RequestCreator<TClientResponse> requestCreator) where TRes : IMessage
        {
            // given
            TestService.AddRequestHandler(
                typeof(TReq),
                req =>
                {
                    throw new RpcException(new Status(StatusCode.Internal, "exhausted"));
                });

            // when
            var resultTask = requestCreator(ZeebeClient).SendWithRetry();
            var aggregateException = Assert.Throws<AggregateException>(() => resultTask.Wait());
            var rpcException = (RpcException) aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(StatusCode.Internal, rpcException.Status.StatusCode);
        }
    }
}