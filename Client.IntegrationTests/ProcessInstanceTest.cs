using System;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Zeebe.Client;

namespace Client.IntegrationTests
{
    [TestFixture]
    public class ProcessInstanceTest
    {
        private static readonly string OneTaskProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "oneTaskProcess.bpmn");
        private static readonly string SimpleProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "simpleProcess.bpmn");
        private static readonly string ProcessInstanceVariables = "{\"a\":123, \"b\":true}";

        private readonly ZeebeIntegrationTestHelper testHelper = ZeebeIntegrationTestHelper.Latest();
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
        public async Task ShouldCreateProcessInstance()
        {
            // given
            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(OneTaskProcessPath)
                .Send();
            var processDefinitionKey = deployResponse.Processes[0].ProcessDefinitionKey;

            // when
            var processInstance = await zeebeClient
                .NewCreateProcessInstanceCommand()
                .ProcessDefinitionKey(processDefinitionKey)
                .Variables(ProcessInstanceVariables)
                .Send();

            // then
            Assert.Equals(processInstance.Version, 1);
            Assert.Equals(processDefinitionKey, processInstance.ProcessDefinitionKey);
            Assert.Equals("oneTaskProcess", processInstance.BpmnProcessId);
            Assert.That(processInstance.ProcessInstanceKey, Is.GreaterThan(1));
        }

        [Test]
        public void ShouldNotCreateProcessInstanceWithoutDeployment()
        {
            // given

            // when
            var aggregateException = Assert.Throws<AggregateException>(() => zeebeClient
                .NewCreateProcessInstanceCommand()
                .ProcessDefinitionKey(1)
                .Variables(ProcessInstanceVariables)
                .Send().Wait());

            // then
            var rpcException = (RpcException) aggregateException.InnerExceptions[0];
            Assert.Equals(StatusCode.NotFound, rpcException.Status.StatusCode);
        }

        [Test]
        public async Task ShouldGetResultAfterCreateProcessInstance()
        {
            // given
            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(SimpleProcessPath)
                .Send();
            var processDefinitionKey = deployResponse.Processes[0].ProcessDefinitionKey;

            // when
            var processInstance = await zeebeClient
                .NewCreateProcessInstanceCommand()
                .ProcessDefinitionKey(processDefinitionKey)
                .Variables(ProcessInstanceVariables)
                .WithResult()
                .Send();

            // then
            Assert.Equals(processInstance.Version, 1);
            Assert.Equals(processDefinitionKey, processInstance.ProcessDefinitionKey);
            Assert.Equals("simpleProcess", processInstance.BpmnProcessId);
            Assert.That(processInstance.ProcessInstanceKey, Is.GreaterThan(1));

            var expectedJson = JObject.Parse(ProcessInstanceVariables);
            var actualJson = JObject.Parse(processInstance.Variables);
            Assert.That(JToken.DeepEquals(expectedJson, actualJson), Is.True);
        }

        [Test]
        public async Task ShouldFetchVariablesOfResult()
        {
            // given
            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(SimpleProcessPath)
                .Send();
            var processDefinitionKey = deployResponse.Processes[0].ProcessDefinitionKey;

            // when
            var processInstance = await zeebeClient
                .NewCreateProcessInstanceCommand()
                .ProcessDefinitionKey(processDefinitionKey)
                .Variables(ProcessInstanceVariables)
                .WithResult()
                .FetchVariables("b")
                .Send();

            // then
            Assert.Equals(processInstance.Version, 1);
            Assert.Equals(processDefinitionKey, processInstance.ProcessDefinitionKey);
            Assert.Equals("simpleProcess", processInstance.BpmnProcessId);
            Assert.That(processInstance.ProcessInstanceKey, Is.GreaterThan(1));

            var expectedJson = new JObject { { "b", true } };
            var actualJson = JObject.Parse(processInstance.Variables);
            Assert.That(JToken.DeepEquals(expectedJson, actualJson), Is.True);
        }

        [Test]
        public async Task ShouldFetchNoVariablesOfResult()
        {
            // given
            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(SimpleProcessPath)
                .Send();
            var processDefinitionKey = deployResponse.Processes[0].ProcessDefinitionKey;

            // when
            var processInstance = await zeebeClient
                .NewCreateProcessInstanceCommand()
                .ProcessDefinitionKey(processDefinitionKey)
                .Variables(ProcessInstanceVariables)
                .WithResult()
                .FetchVariables("c")
                .Send();

            // then
            Assert.Equals(processInstance.Version, 1);
            Assert.Equals(processDefinitionKey, processInstance.ProcessDefinitionKey);
            Assert.Equals("simpleProcess", processInstance.BpmnProcessId);
            Assert.That(processInstance.ProcessInstanceKey, Is.GreaterThan(1));

            var expectedJson = new JObject();
            var actualJson = JObject.Parse(processInstance.Variables);
            Assert.That(JToken.DeepEquals(expectedJson, actualJson), Is.True);
        }
    }
}
