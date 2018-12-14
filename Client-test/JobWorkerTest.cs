//
//    Copyright (c) 2018 camunda services GmbH (info@camunda.com)
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
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

        [Test]
        public void ShouldSendRequestWithTimeSpanTimeout()
        {
            // given
            var expectedRequest = new ActivateJobsRequest
            {
                Timeout = 10_000L,
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
                .Timeout(TimeSpan.FromSeconds(10))
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
                        Worker = "jobWorker",
                        Type = "foo",
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
                        Worker = "jobWorker",
                        Type = "foo",
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
                        Worker = "jobWorker",
                        Type = "foo",
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

        private static void AssertJob(IJob job, int expectedKey)
        {
            Assert.AreEqual(expectedKey, job.Key);
            Assert.AreEqual(3, job.Retries);
            Assert.AreEqual("foo", job.Type);
            Assert.AreEqual("jobWorker", job.Worker);

            Assert.AreEqual("{\"foo\":" + expectedKey + "}", job.Payload);
            var expectedPayload = new Dictionary<string, int> { { "foo", expectedKey } };
            CollectionAssert.AreEquivalent(expectedPayload, job.PayloadAsDictionary);

            Assert.AreEqual("process", job.Headers.BpmnProcessId);
            Assert.AreEqual("job" + expectedKey, job.Headers.ElementId);
            Assert.AreEqual(23, job.Headers.ElementInstanceKey);
            Assert.AreEqual(3, job.Headers.WorkflowDefinitionVersion);
            Assert.AreEqual(21, job.Headers.WorkflowKey);
        }
    }
}