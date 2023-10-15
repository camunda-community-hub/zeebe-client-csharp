using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

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
            const string variables = "{\"foo\":23}";
            var expectedRequest = new ThrowErrorRequest
            {
                JobKey = jobKey,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                Variables = variables
            };

            // when
            await ZeebeClient.NewThrowErrorCommand(jobKey)
                .ErrorCode("code 13")
                .ErrorMessage("This is a business error!")
                .Variables(variables)
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

            // when
            var task = ZeebeClient.NewThrowErrorCommand(jobKey)
                .ErrorCode("code 13")
                .ErrorMessage("This is a business error!")
                .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException)aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(StatusCode.Cancelled, rpcException.Status.StatusCode);
        }
    }
}
