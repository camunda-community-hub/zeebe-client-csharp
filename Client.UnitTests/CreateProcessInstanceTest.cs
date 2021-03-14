using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

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

            // when
            var task = ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .Send(TimeSpan.Zero);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException)aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(Grpc.Core.StatusCode.DeadlineExceeded, rpcException.Status.StatusCode);
        }

        [Test]
        public void ShouldCancelRequest()
        {
            // given

            // when
            var task = ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException)aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(Grpc.Core.StatusCode.Cancelled, rpcException.Status.StatusCode);
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

            TestService.AddRequestHandler(typeof(CreateProcessInstanceRequest), request => expectedResponse);

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
