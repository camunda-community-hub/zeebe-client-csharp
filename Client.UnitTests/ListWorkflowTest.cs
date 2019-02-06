using System.Threading.Tasks;
using GatewayProtocol;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class ListWorkflowTest : BaseZeebeTest
    {
        [Test]
        public async Task shouldSendRequestWithFilterAsExpected()
        {
            // given
            var expected = new ListWorkflowsRequest
            {
                BpmnProcessId = "process"
            };

            // when
            await ZeebeClient.NewListWorkflowRequest().BpmnProcessId("process").Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expected, request);
        }

        [Test]
        public async Task shouldSendRequestAsExpected()
        {
            // given
            var expected = new ListWorkflowsRequest();

            // when
            await ZeebeClient.NewListWorkflowRequest().Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expected, request);
        }

        [Test]
        public async Task shouldReceiveResponseAsExpected()
        {
            // given
            var expectedResponse = new ListWorkflowsResponse();
            expectedResponse.Workflows.Add(new WorkflowMetadata
            {
                BpmnProcessId = "process",
                ResourceName = "file2.bpmn",
                Version = 1,
                WorkflowKey = 2
            });
            expectedResponse.Workflows.Add(new WorkflowMetadata
            {
                BpmnProcessId = "process2",
                ResourceName = "file.bpmn",
                Version = 1,
                WorkflowKey = 3
            });

            TestService.AddRequestHandler(typeof(ListWorkflowsRequest), request => expectedResponse);

            // when
            var response = await ZeebeClient.NewListWorkflowRequest().BpmnProcessId("process").Send();

            // then
            Assert.AreEqual(2, response.WorkflowList.Count);

            var workflowMetadata = response.WorkflowList[0];
            Assert.AreEqual("process", workflowMetadata.BpmnProcessId);
            Assert.AreEqual(1, workflowMetadata.Version);
            Assert.AreEqual("file2.bpmn", workflowMetadata.ResourceName);
            Assert.AreEqual(2, workflowMetadata.WorkflowKey);

            var workflowMetadata2 = response.WorkflowList[1];
            Assert.AreEqual("process2", workflowMetadata2.BpmnProcessId);
            Assert.AreEqual(1, workflowMetadata2.Version);
            Assert.AreEqual("file.bpmn", workflowMetadata2.ResourceName);
            Assert.AreEqual(3, workflowMetadata2.WorkflowKey);
        }
    }
}