using System;
using System.IO;
using System.Threading.Tasks;
using GatewayProtocol;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class WorkflowResourceRequestTest : BaseZeebeTest
    {
        private readonly string DemoProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources","demo-process.bpmn");

        [Test]
        public async Task shouldSendRequestAsExpected()
        {
            // given
            var expectedRequest = new GetWorkflowRequest
            {
                BpmnProcessId = "process",
                Version = -1
            };

            // when
            await ZeebeClient.NewWorkflowResourceRequest()
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
            var expectedRequest = new GetWorkflowRequest
            {
                BpmnProcessId = "process",
                Version = 1
            };

            // when
            await ZeebeClient.NewWorkflowResourceRequest()
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
            var expectedRequest = new GetWorkflowRequest
            {
                WorkflowKey = 1
            };

            // when
            await ZeebeClient.NewWorkflowResourceRequest()
                .WorkflowKey(1)
                .Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }


        [Test]
        public async Task shouldReceiveResponseAsExpected()
        {
            // given
            var expectedResponse = new GetWorkflowResponse
            {
                BpmnProcessId = "process",
                Version = 1,
                WorkflowKey = 2,
                ResourceName = DemoProcessPath,
                BpmnXml = File.ReadAllText(DemoProcessPath)
            };

            TestService.AddRequestHandler(typeof(GetWorkflowRequest), request => expectedResponse);

            // when
            var workflowResourceResponse = await ZeebeClient.NewWorkflowResourceRequest()
                .BpmnProcessId("process")
                .LatestVersion()
                .Send();

            // then
            Assert.AreEqual(2, workflowResourceResponse.WorkflowKey);
            Assert.AreEqual(1, workflowResourceResponse.Version);
            Assert.AreEqual(DemoProcessPath, workflowResourceResponse.ResourceName);
            Assert.AreEqual("process", workflowResourceResponse.BpmnProcessId);
            Assert.AreEqual(File.ReadAllText(DemoProcessPath), workflowResourceResponse.BpmnXml);
        }
    }
}