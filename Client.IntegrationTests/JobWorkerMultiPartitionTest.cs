using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;

namespace Client.IntegrationTests
{
    [TestFixture]
    public class JobWorkerMultiPartitionTest
    {
        private static readonly string DemoProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "oneTaskProcess.bpmn");

        private readonly ZeebeIntegrationTestHelper testHelper = ZeebeIntegrationTestHelper.Latest().withPartitionCount(3);
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
        public async Task ShouldHandleAllJobs()
        {
            // given
            var handledJobs = new List<IJob>();
            foreach (int i in Enumerable.Range(1, 3))
            {
                await zeebeClient.NewCreateProcessInstanceCommand()
                .ProcessDefinitionKey(processDefinitionKey)
                .Send();
            }

            // when
            using (var signal = new EventWaitHandle(false, EventResetMode.AutoReset))
            {
                using (zeebeClient.NewWorker()
                    .JobType("oneTask")
                    .Handler(async (jobClient, job) =>
                    {
                        await jobClient.NewCompleteJobCommand(job).Send();
                        handledJobs.Add(job);
                        if (handledJobs.Count >= 3)
                        {
                            signal.Set();
                        }
                    })
                    .MaxJobsActive(5)
                    .Name("csharpWorker")
                    .Timeout(TimeSpan.FromHours(10))
                    .PollInterval(TimeSpan.FromSeconds(5))
                    .Open())
                {
                        signal.WaitOne(TimeSpan.FromSeconds(5));
                }
            }

            Assert.AreEqual(3, handledJobs.Count);
        }
    }
}
