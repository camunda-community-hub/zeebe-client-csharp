using System;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Zeebe.Client;

namespace Client.IntegrationTests
{
    public class WorkflowInstanceTest
    {
        private static readonly string DemoProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "demo-process.bpmn");
        private static readonly string SimpleProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "simpleProcess.bpmn");
        private static readonly string WorkflowInstanceVariables = "{\"a\":123, \"b\":true}";

        private readonly ZeebeIntegrationTestHelper testHelper = new ZeebeIntegrationTestHelper();
        private IZeebeClient zeebeClient;

        [OneTimeSetUp]
        public async Task Setup()
        {
            zeebeClient = await testHelper.SetupIntegrationTest();
        }

        [OneTimeTearDown]
        public async Task Stop()
        {
            await testHelper.TearDownIntegrationTest();
        }

        [Test]
        public async Task ShouldCreateWorkflowInstance()
        {
            // given
            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(DemoProcessPath)
                .Send();
            var workflowKey = deployResponse.Workflows[0].WorkflowKey;

            // when
            var workflowInstance = await zeebeClient
                .NewCreateWorkflowInstanceCommand()
                .WorkflowKey(workflowKey)
                .Variables(WorkflowInstanceVariables)
                .Send();

            // then
            Assert.AreEqual(workflowInstance.Version, 1);
            Assert.AreEqual(workflowKey, workflowInstance.WorkflowKey);
            Assert.AreEqual("demoProcess", workflowInstance.BpmnProcessId);
            Assert.Greater(workflowInstance.WorkflowInstanceKey, 1);
        }

        [Test]
        public void ShouldNotCreateWorkflowInstanceWithoutDeployment()
        {
            // given

            // when
            var aggregateException = Assert.Throws<AggregateException>(() => zeebeClient
                .NewCreateWorkflowInstanceCommand()
                .WorkflowKey(1)
                .Variables(WorkflowInstanceVariables)
                .Send().Wait());

            // then
            var rpcException = (RpcException) aggregateException.InnerExceptions[0];
            Assert.AreEqual(StatusCode.NotFound, rpcException.Status.StatusCode);
        }

        [Test]
        public async Task ShouldGetResultAfterCreateWorkflowInstance()
        {
            // given
            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(SimpleProcessPath)
                .Send();
            var workflowKey = deployResponse.Workflows[0].WorkflowKey;

            // when
            var workflowInstance = await zeebeClient
                .NewCreateWorkflowInstanceCommand()
                .WorkflowKey(workflowKey)
                .Variables(WorkflowInstanceVariables)
                .WithResult()
                .Send();

            // then
            Assert.AreEqual(workflowInstance.Version, 1);
            Assert.AreEqual(workflowKey, workflowInstance.WorkflowKey);
            Assert.AreEqual("simpleProcess", workflowInstance.BpmnProcessId);
            Assert.Greater(workflowInstance.WorkflowInstanceKey, 1);

            var expectedJson = JObject.Parse(WorkflowInstanceVariables);
            var actualJson = JObject.Parse(workflowInstance.Variables);
            Assert.IsTrue(JToken.DeepEquals(expectedJson, actualJson));
        }

        [Test]
        public async Task ShouldFetchVariablesOfResult()
        {
            // given
            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(SimpleProcessPath)
                .Send();
            var workflowKey = deployResponse.Workflows[0].WorkflowKey;

            // when
            var workflowInstance = await zeebeClient
                .NewCreateWorkflowInstanceCommand()
                .WorkflowKey(workflowKey)
                .Variables(WorkflowInstanceVariables)
                .WithResult()
                .FetchVariables("b")
                .Send();

            // then
            Assert.AreEqual(workflowInstance.Version, 1);
            Assert.AreEqual(workflowKey, workflowInstance.WorkflowKey);
            Assert.AreEqual("simpleProcess", workflowInstance.BpmnProcessId);
            Assert.Greater(workflowInstance.WorkflowInstanceKey, 1);

            var expectedJson = new JObject { { "b", true } };
            var actualJson = JObject.Parse(workflowInstance.Variables);
            Assert.IsTrue(JToken.DeepEquals(expectedJson, actualJson));
        }

        [Test]
        public async Task ShouldFetchNoVariablesOfResult()
        {
            // given
            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(SimpleProcessPath)
                .Send();
            var workflowKey = deployResponse.Workflows[0].WorkflowKey;

            // when
            var workflowInstance = await zeebeClient
                .NewCreateWorkflowInstanceCommand()
                .WorkflowKey(workflowKey)
                .Variables(WorkflowInstanceVariables)
                .WithResult()
                .FetchVariables("c")
                .Send();

            // then
            Assert.AreEqual(workflowInstance.Version, 1);
            Assert.AreEqual(workflowKey, workflowInstance.WorkflowKey);
            Assert.AreEqual("simpleProcess", workflowInstance.BpmnProcessId);
            Assert.Greater(workflowInstance.WorkflowInstanceKey, 1);

            var expectedJson = new JObject();
            var actualJson = JObject.Parse(workflowInstance.Variables);
            Assert.IsTrue(JToken.DeepEquals(expectedJson, actualJson));
        }
    }
}