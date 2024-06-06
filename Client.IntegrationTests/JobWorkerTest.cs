using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;

namespace Client.IntegrationTests
{
    [TestFixture]
    public class JobWorkerTest
    {
        private static readonly string DemoProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "oneTaskProcess.bpmn");

        private readonly ZeebeIntegrationTestHelper testHelper = ZeebeIntegrationTestHelper.Latest();
        private IZeebeClient zeebeClient;
        private long processDefinitionKey;

        [OneTimeSetUp]
        public async Task Setup()
        {
            zeebeClient = await testHelper.SetupIntegrationTest();
            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(DemoProcessPath)
                .Send();
            processDefinitionKey = deployResponse.Processes[0].ProcessDefinitionKey;
        }

        [OneTimeTearDown]
        public async Task Stop()
        {
            await testHelper.TearDownIntegrationTest();
        }

        [Test]
        public async Task ShouldCompleteProcess()
        {
            // given
            var handledJobs = new List<IJob>();

            // when
            using (zeebeClient.NewWorker()
                .JobType("oneTask")
                .Handler(async (jobClient, job) =>
                {
                    handledJobs.Add(job);
                    await jobClient.NewCompleteJobCommand(job).Send();
                })
                .MaxJobsActive(1)
                .Name("ShouldCompleteProcess")
                .Timeout(TimeSpan.FromSeconds(10))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .PollingTimeout(TimeSpan.FromSeconds(30L))
                .Open())
            {
                var processInstance = await zeebeClient.NewCreateProcessInstanceCommand()
                    .ProcessDefinitionKey(processDefinitionKey)
                    .WithResult()
                    .Send();

                // then process was completed
                Assert.Equals(handledJobs.Count, 1);

                Assert.Equals(processInstance.Version, 1);
                Assert.Equals(processDefinitionKey, processInstance.ProcessDefinitionKey);
                Assert.Equals("oneTaskProcess", processInstance.BpmnProcessId);
                Assert.That(processInstance.ProcessInstanceKey, Is.GreaterThan(1));
            }
        }

        [Test]
        public async Task ShouldCompleteProcessWithJobAutoCompletion()
        {
            // given
            var handledJobs = new List<IJob>();

            // when
            using (zeebeClient.NewWorker()
                .JobType("oneTask")
                .Handler((_, job) => handledJobs.Add(job))
                .MaxJobsActive(1)
                .AutoCompletion()
                .Name("ShouldCompleteProcessWithJobAutoCompletion")
                .Timeout(TimeSpan.FromSeconds(10))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .PollingTimeout(TimeSpan.FromSeconds(30L))
                .Open())
            {
                var processInstance = await zeebeClient.NewCreateProcessInstanceCommand()
                    .ProcessDefinitionKey(processDefinitionKey)
                    .WithResult()
                    .Send();

                // then process was completed
                Assert.Equals(1, handledJobs.Count);

                Assert.Equals(processInstance.Version, 1);
                Assert.Equals(processDefinitionKey, processInstance.ProcessDefinitionKey);
                Assert.Equals("oneTaskProcess", processInstance.BpmnProcessId);
                Assert.That(processInstance.ProcessInstanceKey, Is.GreaterThan(1));
            }
        }
    }
}
