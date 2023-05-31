using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;
using Zeebe.Client.Helpers;

namespace Zeebe.Client
{
    [TestFixture]
    public class ThrowErrorTest : BaseZeebeTest
    {
        [Test]
        public async Task ShouldSendRequestAsExpected()
        {
            // given
            const string errorCode = "code 13";
            const string errorMessage = "This is a business error!";
            const int jobKey = 255;
            var expectedRequest = new ThrowErrorRequest
            {
                JobKey = jobKey,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewThrowErrorCommand(jobKey)
                .ErrorCode("code 13")
                .ErrorMessage("This is a business error!")
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(ThrowErrorRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public void ShouldTimeoutRequest()
        {
            // given
            const int jobKey = 255;
            TestService.Reset();

            // when
            var task = ZeebeClient.NewThrowErrorCommand(jobKey)
                .ErrorCode("code 13")
                .ErrorMessage("This is a business error!")
                .Send(TimeSpan.Zero);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException) aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(StatusCode.DeadlineExceeded, rpcException.Status.StatusCode);
        }

        [Test]
        public void ShouldCancelRequest()
        {
            // given
            const int jobKey = 255;
            TestService.Reset();

            // when
            var task = ZeebeClient.NewThrowErrorCommand(jobKey)
                .ErrorCode("code 13")
                .ErrorMessage("This is a business error!")
                .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException) aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(StatusCode.Cancelled, rpcException.Status.StatusCode);
        }
    }
}