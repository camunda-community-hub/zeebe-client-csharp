using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;

namespace Client.IntegrationTests;

[TestFixture]
public class JobWorkerMultiPartitionTest
{
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

    private static readonly string DemoProcessPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "oneTaskProcess.bpmn");

    private readonly ZeebeIntegrationTestHelper testHelper = ZeebeIntegrationTestHelper.Latest().WithPartitionCount(3);
    private IZeebeClient zeebeClient;
    private long processDefinitionKey;

    [Test]
    public async Task ShouldHandleAllJobs()
    {
        // given
        var handledJobs = new List<IJob>();
        foreach (var i in Enumerable.Range(1, 3))
        {
            _ = await zeebeClient.NewCreateProcessInstanceCommand()
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
                           _ = await jobClient.NewCompleteJobCommand(job).Send();
                           handledJobs.Add(job);
                           if (handledJobs.Count >= 3)
                           {
                               _ = signal.Set();
                           }
                       })
                       .MaxJobsActive(5)
                       .Name("csharpWorker")
                       .Timeout(TimeSpan.FromHours(10))
                       .PollInterval(TimeSpan.FromSeconds(5))
                       .Open())
            {
                _ = signal.WaitOne(TimeSpan.FromSeconds(5));
            }
        }

        Assert.AreEqual(3, handledJobs.Count);
    }

    [Test]
    public async Task ShouldActivateAllJobs()
    {
        // given
        foreach (var i in Enumerable.Range(1, 3))
        {
            _ = await zeebeClient.NewCreateProcessInstanceCommand()
                .ProcessDefinitionKey(processDefinitionKey)
                .Send();
        }

        // when
        var activateJobsResponse = await zeebeClient.NewActivateJobsCommand()
            .JobType("oneTask")
            .MaxJobsToActivate(5)
            .WorkerName("csharpWorker")
            .Timeout(TimeSpan.FromHours(10))
            .Send();

        Assert.AreEqual(3, activateJobsResponse.Jobs.Count);
    }
}