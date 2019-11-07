using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client
{
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

            // when
            await ZeebeClient
                .NewUpdateRetriesCommand(1024)
                .Retries(223)
                .Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public void ShouldTimeoutRequest()
        {
            // given

            // when
            var task = ZeebeClient
                .NewUpdateRetriesCommand(1024)
                .Retries(223)
                .Send(TimeSpan.Zero);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException) aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(Grpc.Core.StatusCode.DeadlineExceeded, rpcException.Status.StatusCode);
        }
    }
}