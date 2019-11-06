using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class CreateWorkflowInstanceTest : BaseZeebeTest
    {
        [Test]
        public async Task ShouldSendRequestAsExpected()
        {
            // given
            var expectedRequest = new CreateWorkflowInstanceRequest
            {
                BpmnProcessId = "process",
                Version = -1
            };

            // when
            await ZeebeClient.NewCreateWorkflowInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
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
            var task = ZeebeClient.NewCreateWorkflowInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .Send(TimeSpan.Zero);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException) aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(Grpc.Core.StatusCode.DeadlineExceeded, rpcException.Status.StatusCode);
        }

        [Test]
        public async Task ShouldSendRequestWithVersionAsExpected()
        {
            // given
            var expectedRequest = new CreateWorkflowInstanceRequest
            {
                BpmnProcessId = "process",
                Version = 1
            };

            // when
            await ZeebeClient.NewCreateWorkflowInstanceCommand()
                .BpmnProcessId("process")
                .Version(1)
                .Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldSendRequestWithWorkflowKeyAsExpected()
        {
            // given
            var expectedRequest = new CreateWorkflowInstanceRequest
            {
                WorkflowKey = 1
            };

            // when
            await ZeebeClient.NewCreateWorkflowInstanceCommand()
                .WorkflowKey(1)
                .Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldSendRequestWithVariablesAsExpected()
        {
            // given
            var expectedRequest = new CreateWorkflowInstanceRequest
            {
                WorkflowKey = 1,
                Variables = "{\"foo\":1}"
            };

            // when
            await ZeebeClient.NewCreateWorkflowInstanceCommand()
                .WorkflowKey(1)
                .Variables("{\"foo\":1}")
                .Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldSendRequestWithVariablesAndProcessIdAsExpected()
        {
            // given
            var expectedRequest = new CreateWorkflowInstanceRequest
            {
                BpmnProcessId = "process",
                Version = -1,
                Variables = "{\"foo\":1}"
            };

            // when
            await ZeebeClient.NewCreateWorkflowInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .Variables("{\"foo\":1}")
                .Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldReceiveResponseAsExpected()
        {
            // given
            var expectedResponse = new CreateWorkflowInstanceResponse
            {
                BpmnProcessId = "process",
                Version = 1,
                WorkflowKey = 2,
                WorkflowInstanceKey = 121
            };

            TestService.AddRequestHandler(typeof(CreateWorkflowInstanceRequest), request => expectedResponse);

            // when
            var workflowInstanceResponse = await ZeebeClient.NewCreateWorkflowInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .Send();

            // then
            Assert.AreEqual(2, workflowInstanceResponse.WorkflowKey);
            Assert.AreEqual(1, workflowInstanceResponse.Version);
            Assert.AreEqual(121, workflowInstanceResponse.WorkflowInstanceKey);
            Assert.AreEqual("process", workflowInstanceResponse.BpmnProcessId);
        }
    }
}