using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Zeebe.Client;

namespace Client.IntegrationTests
{
    public class WorkflowTest
    {
        private static readonly string DemoProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "demo-process.bpmn");

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
        public async Task DeployWorkflowTest()
        {
            // given

            // when
            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(DemoProcessPath)
                .Send();

            // then
            Assert.Greater(deployResponse.Key, 0);

            var deployedWorkflows = deployResponse.Workflows;
            Assert.AreEqual(1, deployedWorkflows.Count);

            Assert.AreEqual(1, deployedWorkflows[0].Version);
            Assert.Greater(deployedWorkflows[0].WorkflowKey, 1);
            Assert.AreEqual("demoProcess", deployedWorkflows[0].BpmnProcessId);
            Assert.AreEqual(DemoProcessPath, deployedWorkflows[0].ResourceName);
        }
    }
}