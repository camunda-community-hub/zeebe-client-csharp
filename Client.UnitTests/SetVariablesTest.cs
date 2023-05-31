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
            TestService.Reset();

            // when
            await ZeebeClient.NewSetVariablesCommand(2123).Variables("{\"foo\":\"bar\"}").Send();

            // then
            var request = TestService.Requests[typeof(SetVariablesRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public void ShouldTimeoutRequest()
        {
            // given
            TestService.Reset();

            // when
            var task = ZeebeClient
                .NewSetVariablesCommand(2123)
                .Variables("{\"foo\":\"bar\"}")
                .Send(TimeSpan.Zero);
            var rpcException = Assert.Throws<RpcException>(() => task.GetAwaiter().GetResult());

            // then
            Assert.AreEqual(StatusCode.DeadlineExceeded, rpcException!.Status.StatusCode);
        }

        [Test]
        public void ShouldCancelRequest()
        {
            // given
            TestService.Reset();

            // when
            var task = ZeebeClient
                .NewSetVariablesCommand(2123)
                .Variables("{\"foo\":\"bar\"}")
                .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
            var rpcException = Assert.Throws<RpcException>(() => task.GetAwaiter().GetResult());

            // then
            Assert.AreEqual(StatusCode.Cancelled, rpcException!.Status.StatusCode);
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
            TestService.Reset();

            // when
            await ZeebeClient.NewSetVariablesCommand(2123).Variables("{\"foo\":\"bar\"}").Local().Send();

            // then
            var request = TestService.Requests[typeof(SetVariablesRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldReceiveResponseAsExpected()
        {
            // given
            var expectedResponse = new SetVariablesResponse
            {
                Key = 12
            };
            TestService.AddRequestHandler<SetVariablesRequest>(_ => expectedResponse);

            // when
            var response = await ZeebeClient.NewSetVariablesCommand(2123).Variables("{\"foo\":\"bar\"}").Local().Send();

            // then
            Assert.AreEqual(12, response.Key);
        }
    }
}