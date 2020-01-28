using System;
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
            const int JobKey = 255;
            var expectedRequest = new ThrowErrorRequest
            {
                JobKey = JobKey,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
            };

            // when
            await ZeebeClient.NewThrowErrorCommand(JobKey)
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
            const int JobKey = 255;

            // when
            var task = ZeebeClient.NewThrowErrorCommand(JobKey)
                .ErrorCode("code 13")
                .ErrorMessage("This is a business error!")
                .Send(TimeSpan.Zero);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException) aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(Grpc.Core.StatusCode.DeadlineExceeded, rpcException.Status.StatusCode);
        }
    }
}
