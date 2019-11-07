using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class SetVariablesTest : BaseZeebeTest
    {
        [Test]
        public async Task ShouldSendRequestAsExpected()
        {
            // given
            var expectedRequest = new SetVariablesRequest
            {
                ElementInstanceKey = 2123,
                Variables = "{\"foo\":\"bar\"}"
            };

            // when
            await ZeebeClient.NewSetVariablesCommand(2123).Variables("{\"foo\":\"bar\"}").Send();

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
                .NewSetVariablesCommand(2123)
                .Variables("{\"foo\":\"bar\"}")
                .Send(TimeSpan.Zero);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException) aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(Grpc.Core.StatusCode.DeadlineExceeded, rpcException.Status.StatusCode);
        }

        [Test]
        public async Task ShouldSendRequestWithLocalSemanticsAsExpected()
        {
            // given
            var expectedRequest = new SetVariablesRequest
            {
                ElementInstanceKey = 2123,
                Variables = "{\"foo\":\"bar\"}",
                Local = true
            };

            // when
            await ZeebeClient.NewSetVariablesCommand(2123).Variables("{\"foo\":\"bar\"}").Local().Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }
    }
}