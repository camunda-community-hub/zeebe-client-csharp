using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class FailJobTest : BaseZeebeTest
    {
        [Test]
        public async Task ShouldSendRequestAsExpected()
        {
            // given
            const string errorMessage = "This job failed!";
            const int JobKey = 255;
            var expectedRequest = new FailJobRequest
            {
                JobKey = JobKey,
                ErrorMessage = errorMessage,
                Retries = 2
            };

            // when
            await ZeebeClient.NewFailCommand(JobKey).Retries(2).ErrorMessage("This job failed!").Send();

            // then
            var actualRequest = TestService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public void ShouldTimeoutRequest()
        {
            // given
            const int JobKey = 255;

            // when
            var task = ZeebeClient
                .NewFailCommand(JobKey)
                .Retries(2)
                .ErrorMessage("This job failed!")
                .Send(TimeSpan.Zero);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException) aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(Grpc.Core.StatusCode.DeadlineExceeded, rpcException.Status.StatusCode);
        }
    }
}
