using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client
{
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

        [Test]
        public async Task ShouldRetrySendRequestOnResourceExhaustedUntilSucceed()
        {
            // given
            var countdownEvent = new CountdownEvent(5);
            TestService.AddRequestHandler(
                typeof(CancelProcessInstanceRequest),
                req =>
                {
                    countdownEvent.Signal();
                    throw new RpcException(new Status(StatusCode.ResourceExhausted, "exhausted"));
                });
            var expectedRequest = new CancelProcessInstanceRequest
            {
                ProcessInstanceKey = 12113
            };

            // when
            var resultTask = ZeebeClient.NewCancelInstanceCommand(12113).SendWithRetry();
            countdownEvent.Wait(TimeSpan.FromSeconds(10));

            // then
            Assert.AreEqual(0, countdownEvent.CurrentCount);
            TestService.AddRequestHandler(typeof(CancelProcessInstanceRequest), req => new CancelProcessInstanceResponse());
            await resultTask;

            var request = TestService.Requests[typeof(CancelProcessInstanceRequest)][0];
            Assert.AreEqual(expectedRequest, request);

            var requestCount = TestService.Requests[typeof(CancelProcessInstanceRequest)].Count;
            Assert.GreaterOrEqual(requestCount, 5);
        }

        [Test]
        public async Task ShouldRetrySendRequestOnUnavailableUntilSucceed()
        {
            // given
            var countdownEvent = new CountdownEvent(5);
            TestService.AddRequestHandler(
                typeof(CancelProcessInstanceRequest),
                req =>
                {
                    countdownEvent.Signal();
                    throw new RpcException(new Status(StatusCode.Unavailable, "exhausted"));
                });
            var expectedRequest = new CancelProcessInstanceRequest
            {
                ProcessInstanceKey = 12113
            };

            // when
            var resultTask = ZeebeClient.NewCancelInstanceCommand(12113).SendWithRetry();
            countdownEvent.Wait(TimeSpan.FromSeconds(10));

            // then
            Assert.AreEqual(0, countdownEvent.CurrentCount);
            TestService.AddRequestHandler(typeof(CancelProcessInstanceRequest), req => new CancelProcessInstanceResponse());
            await resultTask;

            var request = TestService.Requests[typeof(CancelProcessInstanceRequest)][0];
            Assert.AreEqual(expectedRequest, request);

            var requestCount = TestService.Requests[typeof(CancelProcessInstanceRequest)].Count;
            Assert.GreaterOrEqual(requestCount, 5);
        }

        [Test]
        public void ShouldNotRetrySendRequest()
        {
            // given
            TestService.AddRequestHandler(
                typeof(CancelProcessInstanceRequest),
                req =>
                {
                    throw new RpcException(new Status(StatusCode.Internal, "exhausted"));
                });

            // when
            var resultTask = ZeebeClient.NewCancelInstanceCommand(12113).SendWithRetry();
            var aggregateException = Assert.Throws<AggregateException>(() => resultTask.Wait());
            var rpcException = (RpcException) aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(StatusCode.Internal, rpcException.Status.StatusCode);
        }
    }
}
