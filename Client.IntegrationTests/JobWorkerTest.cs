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

        private readonly ZeebeIntegrationTestHelper testHelper = new ZeebeIntegrationTestHelper();
        private IZeebeClient zeebeClient;
        private long workflowKey;

        [OneTimeSetUp]
        public async Task Setup()
        {
            zeebeClient = await testHelper.SetupIntegrationTest();
            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(DemoProcessPath)
                .Send();
            workflowKey = deployResponse.Workflows[0].WorkflowKey;
        }

        [OneTimeTearDown]
        public async Task Stop()
        {
            await testHelper.TearDownIntegrationTest();
        }

        [Test]
        public async Task ShouldCompleteWorkflow()
        {
            // given
            var handledJobs = new List<IJob>();

            // when
            using (zeebeClient.NewWorker()
                .JobType("oneTask")
                .Handler(async (jobClient, job) =>
                {
                    await jobClient.NewCompleteJobCommand(job).Send();
                    handledJobs.Add(job);
                })
                .MaxJobsActive(1)
                .Name("csharpWorker")
                .Timeout(TimeSpan.FromSeconds(10))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .PollingTimeout(TimeSpan.FromSeconds(30L))
                .Open())
            {
                var workflowInstance = await zeebeClient.NewCreateWorkflowInstanceCommand()
                    .WorkflowKey(workflowKey)
                    .WithResult()
                    .Send();

                // then workflow was completed
                Assert.AreEqual(1, handledJobs.Count);

                Assert.AreEqual(workflowInstance.Version, 1);
                Assert.AreEqual(workflowKey, workflowInstance.WorkflowKey);
                Assert.AreEqual("oneTaskProcess", workflowInstance.BpmnProcessId);
                Assert.Greater(workflowInstance.WorkflowInstanceKey, 1);
            }
        }

        [Test]
        public async Task ShouldCompleteWorkflowWithJobAutoCompletion()
        {
            // given
            var handledJobs = new List<IJob>();

            // when
            using (zeebeClient.NewWorker()
                .JobType("oneTask")
                .Handler((jobClient, job) => handledJobs.Add(job))
                .MaxJobsActive(1)
                .AutoCompletion()
                .Name("csharpWorker")
                .Timeout(TimeSpan.FromSeconds(10))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .PollingTimeout(TimeSpan.FromSeconds(30L))
                .Open())
            {
                var workflowInstance = await zeebeClient.NewCreateWorkflowInstanceCommand()
                    .WorkflowKey(workflowKey)
                    .WithResult()
                    .Send();

                // then workflow was completed
                Assert.AreEqual(1, handledJobs.Count);

                Assert.AreEqual(workflowInstance.Version, 1);
                Assert.AreEqual(workflowKey, workflowInstance.WorkflowKey);
                Assert.AreEqual("oneTaskProcess", workflowInstance.BpmnProcessId);
                Assert.Greater(workflowInstance.WorkflowInstanceKey, 1);
            }
        }
    }
}