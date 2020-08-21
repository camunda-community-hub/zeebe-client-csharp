using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Microsoft.Extensions.Logging;
using NLog;
using NUnit.Framework;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Worker
{
    [TestFixture]
    public class JobHandlerTest : BaseZeebeTest
    {
        private ConcurrentQueue<IJob> workItems;
        private ConcurrentQueue<IJob> seenJobs;
        private JobWorkerSignal jobWorkerSignal;
        private JobHandlerExecutor jobHandler;
        private CancellationTokenSource tokenSource;

        [SetUp]
        public void SetupTest()
        {
            workItems = new ConcurrentQueue<IJob>();
            seenJobs = new ConcurrentQueue<IJob>();
            var jobWorkerBuilder = new JobWorkerBuilder(ZeebeClient);
            jobWorkerSignal = new JobWorkerSignal();

            jobWorkerBuilder
                .JobType("foo")
                .Handler((jobClient, job) => { seenJobs.Enqueue(job); })
                .MaxJobsActive(3)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromSeconds(123L))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .PollingTimeout(TimeSpan.FromSeconds(5L));
            jobHandler = new JobHandlerExecutor(jobWorkerBuilder, workItems, jobWorkerSignal);
            tokenSource = new CancellationTokenSource();
        }

        [TearDown]
        public async Task CleanUp()
        {
            tokenSource.Cancel();
            // delay disposing, since poll and handler take some time to close
            await Task.Delay(TimeSpan.FromMilliseconds(200))
                .ContinueWith(t => { tokenSource.Dispose(); });

            tokenSource = null;
            jobHandler = null;
            seenJobs = null;
            workItems = null;
            jobWorkerSignal = null;
        }

        [Test]
        public void ShouldHandleJob()
        {
            // given
            var expectedJob = CreateActivatedJob(1);
            workItems.Enqueue(CreateActivatedJob(1));

            // when
            ScheduleHandling();

            // then
            var hasJobHandled = jobWorkerSignal.AwaitJobHandling(TimeSpan.FromSeconds(1));
            Assert.IsTrue(hasJobHandled);
            AwaitJobsHaveSeen(1);

            Assert.AreEqual(1, seenJobs.Count);
            Assert.IsTrue(seenJobs.TryDequeue(out var actualJob));
            Assert.AreEqual(expectedJob, actualJob);
        }

        [Test]
        public void ShouldTriggerJobHandling()
        {
            // given
            var expectedJob = CreateActivatedJob(1);
            ScheduleHandling();
            jobWorkerSignal.AwaitJobHandling(TimeSpan.FromSeconds(1));

            // when
            workItems.Enqueue(CreateActivatedJob(1));
            jobWorkerSignal.SignalJobPolled();

            // then
            var hasJobHandled = jobWorkerSignal.AwaitJobHandling(TimeSpan.FromSeconds(1));
            Assert.IsTrue(hasJobHandled);
            AwaitJobsHaveSeen(1);

            Assert.AreEqual(1, seenJobs.Count);
            Assert.IsTrue(seenJobs.TryDequeue(out var actualJob));
            Assert.AreEqual(expectedJob, actualJob);
        }

        [Test]
        public void ShouldHandleJobsInOrder()
        {
            // given
            workItems.Enqueue(CreateActivatedJob(1));
            workItems.Enqueue(CreateActivatedJob(2));
            workItems.Enqueue(CreateActivatedJob(3));

            // when
            ScheduleHandling();

            // then
            AwaitJobsHaveSeen(3);

            IJob actualJob;
            Assert.IsTrue(seenJobs.TryDequeue(out actualJob));
            Assert.AreEqual(1, actualJob.Key);
            Assert.IsTrue(seenJobs.TryDequeue(out actualJob));
            Assert.AreEqual(2, actualJob.Key);
            Assert.IsTrue(seenJobs.TryDequeue(out actualJob));
            Assert.AreEqual(3, actualJob.Key);
        }

        [Test]
        public void ShouldNotHandleDuplicateOnConcurrentHandlers()
        {
            // given
            workItems.Enqueue(CreateActivatedJob(1));
            workItems.Enqueue(CreateActivatedJob(2));
            workItems.Enqueue(CreateActivatedJob(3));

            // when
            ScheduleHandling();
            ScheduleHandling();

            // then
            AwaitJobsHaveSeen(3);
            CollectionAssert.AllItemsAreUnique(seenJobs);
        }

        private async void AwaitJobsHaveSeen(int expectedCount)
        {
            while (!tokenSource.IsCancellationRequested && seenJobs.Count < expectedCount)
            {
                await Task.Delay(25);
            }
        }

        private void ScheduleHandling()
        {
            Task.Run(() => jobHandler.HandleActivatedJobs(tokenSource.Token), tokenSource.Token);
        }

        private static Responses.ActivatedJob CreateActivatedJob(long key)
        {
            return new Responses.ActivatedJob(new ActivatedJob
            {
                Key = key,
                Worker = "jobWorker",
                Type = "foo",
                Variables = "{\"foo\":1}",
                CustomHeaders = "{\"customFoo\":\"1\"}",
                Retries = 3,
                Deadline = 123932,
                BpmnProcessId = "process",
                ElementId = "job1",
                ElementInstanceKey = 23,
                WorkflowInstanceKey = 29,
                WorkflowDefinitionVersion = 3,
                WorkflowKey = 21
            });
        }
    }
}