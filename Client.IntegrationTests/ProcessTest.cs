using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Zeebe.Client;

namespace Client.IntegrationTests
{
    [TestFixture]
    public class ProcessTest
    {
        private static readonly string DemoProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "simpleProcess.bpmn");

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
        public async Task DeployProcessTest()
        {
            // given

            // when
            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(DemoProcessPath)
                .Send();

            // then
            Assert.That(deployResponse.Key, Is.GreaterThan(0));

            var deployedProcesses = deployResponse.Processes;
            Assert.Equals(1, deployedProcesses.Count);

            Assert.Equals(1, deployedProcesses[0].Version);
            Assert.That(deployedProcesses[0].ProcessDefinitionKey, Is.GreaterThan(1));

            Assert.Equals("simpleProcess", deployedProcesses[0].BpmnProcessId);
            Assert.Equals(DemoProcessPath, deployedProcesses[0].ResourceName);
        }
    }
}
