using System;
using System.Collections.Concurrent;
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
    public class JobPollerTest : BaseZeebeTest
    {
        private ConcurrentQueue<IJob> workItems;
        private JobWorkerSignal jobWorkerSignal;
        private JobPoller jobPoller;
        private CancellationTokenSource tokenSource;

        [SetUp]
        public void SetupTest()
        {
            workItems = new ConcurrentQueue<IJob>();
            var jobWorkerBuilder = new JobWorkerBuilder(ZeebeClient);
            jobWorkerSignal = new JobWorkerSignal();
            jobWorkerBuilder
                .JobType("foo")
                .Handler((jobClient, job) => { })
                .MaxJobsActive(3)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromSeconds(123L))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .PollingTimeout(TimeSpan.FromSeconds(5L));
            jobPoller = new JobPoller(jobWorkerBuilder, workItems, jobWorkerSignal);
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
            jobWorkerSignal = null;
            workItems = null;
            jobPoller = null;
        }

        [Test]
        public void ShouldSendRequests()
        {
            // given
            var expectedRequest = new ActivateJobsRequest
            {
                Timeout = 123_000L,
                MaxJobsToActivate = 3,
                Type = "foo",
                Worker = "jobWorker",
                RequestTimeout = 5_000L
            };

            // when
            SchedulePolling();

            // then
            var hasPolled = jobWorkerSignal.AwaitNewJobPolled(TimeSpan.FromSeconds(1));
            Assert.IsTrue(hasPolled);
            var actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public void ShouldSendRequestsImmediatelyAfterEmptyResponse()
        {
            // given
            var expectedRequest = new ActivateJobsRequest
            {
                Timeout = 123_000L,
                MaxJobsToActivate = 3,
                Type = "foo",
                Worker = "jobWorker",
                RequestTimeout = 5_000L
            };
            SchedulePolling();

            // when
            var hasPolled = jobWorkerSignal.AwaitNewJobPolled(TimeSpan.FromSeconds(5));
            var hasPolledSecondTime = jobWorkerSignal.AwaitNewJobPolled(TimeSpan.FromSeconds(5));

            // then
            Assert.IsTrue(hasPolled);
            Assert.IsTrue(hasPolledSecondTime);

            Assert.GreaterOrEqual(TestService.Requests[typeof(ActivateJobsRequest)].Count, 2);

            var actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualRequest);

            actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][1];
            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public void ShouldPutActivatedJobsIntoQueue()
        {
            // given
            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => JobWorkerTest.CreateExpectedResponse());

            // when
            SchedulePolling();

            // then
            var hasPolled = jobWorkerSignal.AwaitNewJobPolled(TimeSpan.FromSeconds(1));
            Assert.IsTrue(hasPolled);
            Assert.AreEqual(workItems.Count, 3);
        }

        [Test]
        public void ShouldNotPollNewJobsWhenQueueIsFull()
        {
            // given
            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => JobWorkerTest.CreateExpectedResponse());
            SchedulePolling();
            jobWorkerSignal.AwaitNewJobPolled(TimeSpan.FromSeconds(1));

            // when
            jobWorkerSignal.SignalJobHandled();

            // then
            Assert.AreEqual(TestService.Requests[typeof(ActivateJobsRequest)].Count, 1);
        }

        [Test]
        public void ShouldNotPollNewJobsWhenThresholdIsNotMet()
        {
            // given
            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => JobWorkerTest.CreateExpectedResponse());
            SchedulePolling();
            jobWorkerSignal.AwaitNewJobPolled(TimeSpan.FromSeconds(1));
            workItems.TryDequeue(out _);

            // when
            jobWorkerSignal.SignalJobHandled();

            // then
            Assert.AreEqual(TestService.Requests[typeof(ActivateJobsRequest)].Count, 1);
        }

        [Test]
        public void ShouldPollNewJobsWhenThresholdIsMet()
        {
            // given
            var expectedSecondRequest = new ActivateJobsRequest
            {
                Timeout = 123_000L,
                MaxJobsToActivate = 2,
                Type = "foo",
                Worker = "jobWorker",
                RequestTimeout = 5_000L
            };
            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => JobWorkerTest.CreateExpectedResponse());
            SchedulePolling();
            jobWorkerSignal.AwaitNewJobPolled(TimeSpan.FromSeconds(1));
            workItems.TryDequeue(out _);
            workItems.TryDequeue(out _);

            // when
            jobWorkerSignal.SignalJobHandled();
            jobWorkerSignal.AwaitNewJobPolled(TimeSpan.FromSeconds(1));

            // then
            Assert.AreEqual(2, TestService.Requests[typeof(ActivateJobsRequest)].Count);
            var actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][1];
            Assert.AreEqual(expectedSecondRequest, actualRequest);
        }

        [Test]
        public void ShouldPollNewJobsAfterQueueIsCleared()
        {
            // given
            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => JobWorkerTest.CreateExpectedResponse());
            SchedulePolling();
            jobWorkerSignal.AwaitNewJobPolled(TimeSpan.FromSeconds(1));

            // when
            workItems.Clear();
            jobWorkerSignal.SignalJobHandled();
            jobWorkerSignal.AwaitNewJobPolled(TimeSpan.FromSeconds(1));

            // then
            Assert.AreEqual(2, TestService.Requests[typeof(ActivateJobsRequest)].Count);
            Assert.AreEqual(3, workItems.Count);
        }

        [Test]
        public void ShouldPollNewJobsAfterQueueIsClearedAndPollIntervalIsDue()
        {
            // given
            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => JobWorkerTest.CreateExpectedResponse());
            SchedulePolling();
            jobWorkerSignal.AwaitNewJobPolled(TimeSpan.FromSeconds(1));

            // when
            workItems.Clear();
            jobWorkerSignal.AwaitNewJobPolled(TimeSpan.FromSeconds(1));

            // then
            AwaitRequestCount(typeof(ActivateJobsRequest), 2);
            Assert.AreEqual(2, TestService.Requests[typeof(ActivateJobsRequest)].Count);
            Assert.AreEqual(3, workItems.Count);
        }

        private void SchedulePolling()
        {
            Task.Run(() => jobPoller.Poll(tokenSource.Token), tokenSource.Token);
        }

        [Test]
        public void ShouldTimeoutAndRetry()
        {
            // given
            var jobWorkerBuilder = new JobWorkerBuilder(ZeebeClient);
            jobWorkerBuilder
                .JobType("foo")
                .Handler((jobClient, job) => { })
                .MaxJobsActive(3)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromSeconds(123L))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                // timeout will be + 10 seconds
                .PollingTimeout(TimeSpan.FromMilliseconds(5L));
            jobPoller = new JobPoller(jobWorkerBuilder, workItems, jobWorkerSignal);
            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request =>
            {
                // doesn't send response back before timeout
                jobWorkerSignal.AwaitJobHandling(TimeSpan.FromMinutes(1));
                return JobWorkerTest.CreateExpectedResponse();
            });
            SchedulePolling();

            // when
            var polled = jobWorkerSignal.AwaitNewJobPolled(TimeSpan.FromSeconds(15));

            // then
            Assert.IsFalse(polled);
            Assert.AreEqual(2, TestService.Requests[typeof(ActivateJobsRequest)].Count);
            Assert.AreEqual(0, workItems.Count);
        }

        [Test]
        public void ShouldImmediatelyRetryOnServerException()
        {
            // given
            var jobWorkerBuilder = new JobWorkerBuilder(ZeebeClient);
            jobWorkerBuilder
                .JobType("foo")
                .Handler((jobClient, job) => { })
                .MaxJobsActive(3)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromSeconds(123L))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .PollingTimeout(TimeSpan.FromMilliseconds(5L));
            jobPoller = new JobPoller(jobWorkerBuilder, workItems, jobWorkerSignal);
            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request =>
            {
                throw new Exception("Server dies.");
            });
            SchedulePolling();

            // when
            var polled = jobWorkerSignal.AwaitNewJobPolled(TimeSpan.FromSeconds(1));

            // then
            Assert.IsFalse(polled);
            Assert.GreaterOrEqual(2, TestService.Requests[typeof(ActivateJobsRequest)].Count);
            Assert.AreEqual(0, workItems.Count);
        }
    }
}