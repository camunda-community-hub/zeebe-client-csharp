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
    public class CreateProcessInstanceTest : BaseZeebeTest
    {
        [Test]
        public async Task ShouldSendRequestAsExpected()
        {
            // given
            var expectedRequest = new CreateProcessInstanceRequest
            {
                BpmnProcessId = "process",
                Version = -1
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .Send();

            // then
            var request = TestService.Requests[typeof(CreateProcessInstanceRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public void ShouldTimeoutRequest()
        {
            // given
            TestService.Reset();

            // when
            var task = ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
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
            var task = ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
            var rpcException = Assert.Throws<RpcException>(() => task.GetAwaiter().GetResult());

            // then
            Assert.AreEqual(StatusCode.Cancelled, rpcException!.Status.StatusCode);
        }

        [Test]
        public async Task ShouldSendRequestWithVersionAsExpected()
        {
            // given
            var expectedRequest = new CreateProcessInstanceRequest
            {
                BpmnProcessId = "process",
                Version = 1
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .Version(1)
                .Send();

            // then
            var request = TestService.Requests[typeof(CreateProcessInstanceRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldSendRequestWithProcessDefinitionKeyAsExpected()
        {
            // given
            var expectedRequest = new CreateProcessInstanceRequest
            {
                ProcessDefinitionKey = 1
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewCreateProcessInstanceCommand()
                .ProcessDefinitionKey(1)
                .Send();

            // then
            var request = TestService.Requests[typeof(CreateProcessInstanceRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldSendRequestWithVariablesAsExpected()
        {
            // given
            var expectedRequest = new CreateProcessInstanceRequest
            {
                ProcessDefinitionKey = 1,
                Variables = "{\"foo\":1}"
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewCreateProcessInstanceCommand()
                .ProcessDefinitionKey(1)
                .Variables("{\"foo\":1}")
                .Send();

            // then
            var request = TestService.Requests[typeof(CreateProcessInstanceRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldSendRequestWithVariablesAndProcessIdAsExpected()
        {
            // given
            var expectedRequest = new CreateProcessInstanceRequest
            {
                BpmnProcessId = "process",
                Version = -1,
                Variables = "{\"foo\":1}"
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .Variables("{\"foo\":1}")
                .Send();

            // then
            var request = TestService.Requests[typeof(CreateProcessInstanceRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldReceiveResponseAsExpected()
        {
            // given
            var expectedResponse = new CreateProcessInstanceResponse
            {
                BpmnProcessId = "process",
                Version = 1,
                ProcessDefinitionKey = 2,
                ProcessInstanceKey = 121
            };

            TestService.AddRequestHandler<CreateProcessInstanceRequest>(_ => expectedResponse);

            // when
            var processInstanceResponse = await ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .Send();

            // then
            Assert.AreEqual(2, processInstanceResponse.ProcessDefinitionKey);
            Assert.AreEqual(1, processInstanceResponse.Version);
            Assert.AreEqual(121, processInstanceResponse.ProcessInstanceKey);
            Assert.AreEqual("process", processInstanceResponse.BpmnProcessId);
        }
    }
}