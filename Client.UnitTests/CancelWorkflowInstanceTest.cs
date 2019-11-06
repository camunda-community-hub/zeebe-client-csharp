using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class CancelWorkflowInstanceTest : BaseZeebeTest
    {
        [Test]
        public async Task ShouldSendRequestAsExpected()
        {
            // given
            var expected = new CancelWorkflowInstanceRequest
            {
                WorkflowInstanceKey = 12113
            };

            // when
            await ZeebeClient.NewCancelInstanceCommand(12113).Send();

            // then
            var request = TestService.Requests[0];
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
            var rpcException = (RpcException) aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(Grpc.Core.StatusCode.DeadlineExceeded, rpcException.Status.StatusCode);
        }
    }
}