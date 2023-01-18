using System;
using System.Threading;
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
            const int jobKey = 255;
            var expectedRequest = new FailJobRequest
            {
                JobKey = jobKey,
                ErrorMessage = errorMessage,
                Retries = 2,
                RetryBackOff = 1562
            };

            // when
            await ZeebeClient.NewFailCommand(jobKey)
                .Retries(2)
                .ErrorMessage("This job failed!")
                .RetryBackOff(TimeSpan.FromMilliseconds(1562.5))
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(FailJobRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public void ShouldTimeoutRequest()
        {
            // given
            const int jobKey = 255;

            // when
            var task = ZeebeClient
                .NewFailCommand(jobKey)
                .Retries(2)
                .ErrorMessage("This job failed!")
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
            const int jobKey = 255;

            // when
            var task = ZeebeClient
                .NewFailCommand(jobKey)
                .Retries(2)
                .ErrorMessage("This job failed!")
                .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException)aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(StatusCode.Cancelled, rpcException.Status.StatusCode);
        }
    }
}
