using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;
using static Zeebe.Client.JobWorkerTest;

namespace Zeebe.Client
{
    [TestFixture]
    public class ActivateJobTest : BaseZeebeTest
    {
        [Test]
        public async Task ShouldSendRequestReceiveResponseAsExpected()
        {
            // given
            var expectedRequest = new ActivateJobsRequest
            {
                Timeout = 10_000L,
                MaxJobsToActivate = 1,
                Type = "foo",
                Worker = "jobWorker",
                RequestTimeout = 5_000L
            };

            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => CreateExpectedResponse());

            // when
            var response = await ZeebeClient.NewActivateJobsCommand()
                .JobType("foo")
                .MaxJobsToActivate(1)
                .Timeout(TimeSpan.FromSeconds(10))
                .WorkerName("jobWorker")
                .PollingTimeout(TimeSpan.FromSeconds(5))
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualRequest);

            var receivedJobs = response.Jobs;
            Assert.AreEqual(receivedJobs.Count, 3);

            AssertJob(receivedJobs[0], 1);
            AssertJob(receivedJobs[1], 2);
            AssertJob(receivedJobs[2], 3);
        }

        [Test]
        public void ShouldTimeoutRequest()
        {
            // given

            // when
            var task = ZeebeClient.NewActivateJobsCommand()
                .JobType("foo")
                .MaxJobsToActivate(1)
                .Timeout(TimeSpan.FromSeconds(10))
                .WorkerName("jobWorker")
                .PollingTimeout(TimeSpan.FromSeconds(5))
                .Send(TimeSpan.Zero);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException)aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(Grpc.Core.StatusCode.DeadlineExceeded, rpcException.Status.StatusCode);
        }

        [Test]
        public void ShouldCancelRequest()
        {
            // given

            // when
            var task = ZeebeClient.NewActivateJobsCommand()
                .JobType("foo")
                .MaxJobsToActivate(1)
                .Timeout(TimeSpan.FromSeconds(10))
                .WorkerName("jobWorker")
                .PollingTimeout(TimeSpan.FromSeconds(5))
                .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException)aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(Grpc.Core.StatusCode.Cancelled, rpcException.Status.StatusCode);
        }

        [Test]
        public async Task ShouldSendRequestWithFetchVariablesReceiveResponseAsExpected()
        {
            // given
            var expectedRequest = new ActivateJobsRequest
            {
                Timeout = 10_000L,
                MaxJobsToActivate = 1,
                Type = "foo",
                Worker = "jobWorker",
                FetchVariable = { "foo", "bar", "test" },
                RequestTimeout = 1_000L
            };

            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => CreateExpectedResponse());

            // when
            var response = await ZeebeClient.NewActivateJobsCommand()
                .JobType("foo")
                .MaxJobsToActivate(1)
                .Timeout(TimeSpan.FromSeconds(10))
                .WorkerName("jobWorker")
                .FetchVariables("foo", "bar", "test")
                .PollingTimeout(TimeSpan.FromMilliseconds(1_000L))
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualRequest);

            var receivedJobs = response.Jobs;
            Assert.AreEqual(receivedJobs.Count, 3);

            AssertJob(receivedJobs[0], 1);
            AssertJob(receivedJobs[1], 2);
            AssertJob(receivedJobs[2], 3);
        }

        [Test]
        public async Task ShouldSendRequestWithFetchVariablesListReceiveResponseAsExpected()
        {
            // given
            var expectedRequest = new ActivateJobsRequest
            {
                Timeout = 10_000L,
                MaxJobsToActivate = 1,
                Type = "foo",
                Worker = "jobWorker",
                FetchVariable = { "foo", "bar", "test" },
                RequestTimeout = 5_000L
            };
            IList<string> variableNames = new List<string> { "foo", "bar", "test" };
            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => CreateExpectedResponse());

            // when
            var response = await ZeebeClient.NewActivateJobsCommand()
                .JobType("foo")
                .MaxJobsToActivate(1)
                .Timeout(TimeSpan.FromSeconds(10))
                .WorkerName("jobWorker")
                .FetchVariables(variableNames)
                .PollingTimeout(TimeSpan.FromSeconds(5))
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualRequest);

            var receivedJobs = response.Jobs;
            Assert.AreEqual(receivedJobs.Count, 3);

            AssertJob(receivedJobs[0], 1);
            AssertJob(receivedJobs[1], 2);
            AssertJob(receivedJobs[2], 3);
        }

        [Test]
        public async Task ShouldSendRequestWithTenantIdsListReceiveResponseAsExpected()
        {
            // given
            var expectedRequest = new ActivateJobsRequest
            {
                Type = "foo",
                Worker = "jobWorker",
                Timeout = 10_000L,
                MaxJobsToActivate = 1,
                RequestTimeout = 5_000L,
                TenantIds = { "1234", "5678" }
            };

            IList<string> tenantIds = new List<string> { "1234", "5678" };
            TestService.AddRequestHandler(typeof(ActivateJobsRequest), _ => CreateExpectedResponse());

            // when
            var response = await ZeebeClient.NewActivateJobsCommand()
                .AddTenantIds(tenantIds)
                .JobType("foo")
                .MaxJobsToActivate(1)
                .Timeout(TimeSpan.FromSeconds(10))
                .WorkerName("jobWorker")
                .PollingTimeout(TimeSpan.FromSeconds(5))
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualRequest);

            var receivedJobs = response.Jobs;
            Assert.AreEqual(receivedJobs.Count, 3);

            AssertJob(receivedJobs[0], 1);
            AssertJob(receivedJobs[1], 2);
            AssertJob(receivedJobs[2], 3);
        }
    }
}
