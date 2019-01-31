using System.Threading.Tasks;
using GatewayProtocol;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class CreateWorkflowInstanceTest : BaseZeebeTest
    {
        [Test]
        public async Task shouldSendRequestAsExpected()
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
        public async Task shouldSendRequestWithVersionAsExpected()
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
        public async Task shouldSendRequestWithWorkflowKeyAsExpected()
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
        public async Task shouldSendRequestWithPayloadAsExpected()
        {
            // given
            var expectedRequest = new CreateWorkflowInstanceRequest
            {
                WorkflowKey = 1,
                Payload = "{\"foo\":1}"
            };

            // when
            await ZeebeClient.NewCreateWorkflowInstanceCommand()
                .WorkflowKey(1)
                .Payload("{\"foo\":1}")
                .Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task shouldSendRequestWithPayloadAndProcessIdAsExpected()
        {
            // given
            var expectedRequest = new CreateWorkflowInstanceRequest
            {
                BpmnProcessId = "process",
                Version = -1,
                Payload = "{\"foo\":1}"
            };

            // when
            await ZeebeClient.NewCreateWorkflowInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .Payload("{\"foo\":1}")
                .Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }


        [Test]
        public async Task shouldReceiveResponseAsExpected()
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