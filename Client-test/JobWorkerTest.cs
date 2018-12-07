using GatewayProtocol;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client
{
    [TestFixture]
    public class JobWorkerTest : BaseZeebeTest
    {
        [Test]
        public void ShouldSendRequestReceiveResponseAsExpected()
        {
            // given
            var expectedRequest = new ActivateJobsRequest
            {
                Timeout = 123L,
                Amount = 1,
                Type = "foo",
                Worker = "jobWorker"
            };

            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => CreateExpectedResponse());

            // when
            var signal = new EventWaitHandle(false, EventResetMode.AutoReset);
            var receivedJobs = new List<IJob>();
            using (var jobWorker = ZeebeClient.NewWorker()
                .JobType("foo")
                .Handler((jobClient, job) =>
                {
                    receivedJobs.Add(job);
                    if (receivedJobs.Count == 3)
                    {
                        signal.Set();
                    }
                })
                .Limit(1)
                .Name("jobWorker")
                .Timeout(123L)
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .Open())
            {

                Assert.True(jobWorker.IsOpen());
                signal.WaitOne();
            }

            // then
            var actualRequest = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, actualRequest);

            Assert.AreEqual(receivedJobs.Count, 3);

            AssertJob(receivedJobs[0], 1);
            AssertJob(receivedJobs[1], 2);
            AssertJob(receivedJobs[2], 3);
        }

        private static ActivateJobsResponse CreateExpectedResponse()
        {
            return new ActivateJobsResponse
            {
                Jobs =
                {
                    new ActivatedJob{
                        Key = 1,
                        Payload = "{\"foo\":1}",
                        Retries = 3,
                        Deadline = 123932,
                        JobHeaders = new JobHeaders{
                            BpmnProcessId = "process",
                            ElementId = "job1",
                            ElementInstanceKey = 23,
                            WorkflowDefinitionVersion = 3,
                            WorkflowKey = 21}
                    },
                    new ActivatedJob{
                        Key = 2,
                        Payload = "{\"foo\":2}",
                        Retries = 3,
                        Deadline = 123932,
                        JobHeaders = new JobHeaders{
                            BpmnProcessId = "process",
                            ElementId = "job2",
                            ElementInstanceKey = 23,
                            WorkflowDefinitionVersion = 3,
                            WorkflowKey = 21}
                    },
                    new ActivatedJob{
                        Key = 3,
                        Payload = "{\"foo\":3}",
                        Retries = 3,
                        Deadline = 123932,
                        JobHeaders = new JobHeaders{
                            BpmnProcessId = "process",
                            ElementId = "job3",
                            ElementInstanceKey = 23,
                            WorkflowDefinitionVersion = 3,
                            WorkflowKey = 21}
                    }
                }
            };
        }

        private static void AssertJob(IJob firstJob, int expectedKey)
        {
            Assert.AreEqual(expectedKey, firstJob.Key);
            Assert.AreEqual(3, firstJob.Retries);

            Assert.AreEqual("{\"foo\":" + expectedKey + "}", firstJob.Payload);
            var expectedPayload = new Dictionary<String, int> { { "foo", expectedKey } };
            CollectionAssert.AreEquivalent(expectedPayload, firstJob.PayloadAsDictionary);

            Assert.AreEqual("process", firstJob.Headers.BpmnProcessId);
            Assert.AreEqual("job" + expectedKey, firstJob.Headers.ElementId);
            Assert.AreEqual(23, firstJob.Headers.ElementInstanceKey);
            Assert.AreEqual(3, firstJob.Headers.WorkflowDefinitionVersion);
            Assert.AreEqual(21, firstJob.Headers.WorkflowKey);
        }
    }
}