using GatewayProtocol;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            };

            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => CreateExpectedResponse());

            // when
            var response = await ZeebeClient.NewActivateJobsCommand()
                .JobType("foo")
                .MaxJobsToActivate(1)
                .Timeout(TimeSpan.FromSeconds(10))
                .WorkerName("jobWorker")
                .Send();

            // then
            var actualRequest = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, actualRequest);

            var receivedJobs = response.Jobs;
            Assert.AreEqual(receivedJobs.Count, 3);

            AssertJob(receivedJobs[0], 1);
            AssertJob(receivedJobs[1], 2);
            AssertJob(receivedJobs[2], 3);
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
                FetchVariable = { "foo", "bar", "test" }
            };

            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => CreateExpectedResponse());

            // when
            var response = await ZeebeClient.NewActivateJobsCommand()
                .JobType("foo")
                .MaxJobsToActivate(1)
                .Timeout(TimeSpan.FromSeconds(10))
                .WorkerName("jobWorker")
                .FetchVariables("foo", "bar", "test")
                .Send();

            // then
            var actualRequest = TestService.Requests[0];
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
                FetchVariable = { "foo", "bar", "test" }
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
                .Send();

            // then
            var actualRequest = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, actualRequest);

            var receivedJobs = response.Jobs;
            Assert.AreEqual(receivedJobs.Count, 3);

            AssertJob(receivedJobs[0], 1);
            AssertJob(receivedJobs[1], 2);
            AssertJob(receivedJobs[2], 3);
        }
    }
}
