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

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GatewayProtocol;
using NLog;
using NUnit.Framework;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client
{
    [TestFixture]
    public class JobWorkerTest : BaseZeebeTest
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Test]
        public void ShouldSendRequestReceiveResponseAsExpected()
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
                .MaxJobsActive(3)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromSeconds(123L))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .PollingTimeout(TimeSpan.FromSeconds(5L))
                .Open())
            {
                Assert.True(jobWorker.IsOpen());
                signal.WaitOne();
            }

            // then
            var actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualRequest);

            Assert.AreEqual(receivedJobs.Count, 3);

            AssertJob(receivedJobs[0], 1);
            AssertJob(receivedJobs[1], 2);
            AssertJob(receivedJobs[2], 3);
        }

        [Test]
        public void ShouldFailWithZeroThreadCount()
        {
            // expected
            var aggregateException = Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    ZeebeClient.NewWorker()
                        .JobType("foo")
                        .Handler((jobClient, job) => { })
                        .HandlerThreads(0);
                });
            StringAssert.Contains("Expected an handler thread count larger then zero, but got 0.", aggregateException.Message);
        }

        [Test]
        public void ShouldSendAsyncCompleteInHandler()
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

            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => CreateExpectedResponse());

            // when
            var signal = new EventWaitHandle(false, EventResetMode.AutoReset);
            var completedJobs = new List<IJob>();
            using (var jobWorker = ZeebeClient.NewWorker()
                .JobType("foo")
                .Handler(async (jobClient, job) =>
                {
                    await jobClient.NewCompleteJobCommand(job).Send();
                    completedJobs.Add(job);
                    if (completedJobs.Count == 3)
                    {
                        signal.Set();
                    }
                })
                .MaxJobsActive(3)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromSeconds(123L))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .PollingTimeout(TimeSpan.FromSeconds(5L))
                .HandlerThreads(1)
                .Open())
            {
                Assert.True(jobWorker.IsOpen());
                signal.WaitOne();
            }

            // then
            var actualActivateRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualActivateRequest);

            var completeRequests = TestService.Requests[typeof(CompleteJobRequest)];
            Assert.GreaterOrEqual(completeRequests.Count, 3);
            Assert.GreaterOrEqual(completedJobs.Count, 3);
            AssertJob(completedJobs[0], 1);
            AssertJob(completedJobs[1], 2);
            AssertJob(completedJobs[2], 3);
        }

        [Test]
        public void ShouldUseMultipleHandlerThreads()
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

            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => CreateExpectedResponse());

            // when
            var signal = new EventWaitHandle(false, EventResetMode.AutoReset);
            var completedJobs = new ConcurrentDictionary<long, IJob>();
            using (var jobWorker = ZeebeClient.NewWorker()
                .JobType("foo")
                .Handler(async (jobClient, job) =>
                {
                    await jobClient.NewCompleteJobCommand(job).Send();
                    completedJobs.TryAdd(job.Key, job);
                    if (completedJobs.Count == 3)
                    {
                        signal.Set();
                    }
                })
                .MaxJobsActive(3)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromSeconds(123L))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .PollingTimeout(TimeSpan.FromSeconds(5L))
                .HandlerThreads(3)
                .Open())
            {
                Assert.True(jobWorker.IsOpen());
                signal.WaitOne();
            }

            // then
            var actualActivateRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualActivateRequest);

            var completeRequests = TestService.Requests[typeof(CompleteJobRequest)];
            Assert.GreaterOrEqual(completeRequests.Count, 3);
            Assert.GreaterOrEqual(completedJobs.Count, 3);
            CollectionAssert.AreEquivalent(new List<long> { 1, 2, 3 }, completedJobs.Keys);
        }

        [Test]
        public void ShouldSendCompleteInHandler()
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

            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => CreateExpectedResponse());

            // when
            var signal = new EventWaitHandle(false, EventResetMode.AutoReset);
            var completedJobs = new List<IJob>();
            using (var jobWorker = ZeebeClient.NewWorker()
                .JobType("foo")
                .Handler((jobClient, job) =>
                {
                    jobClient.NewCompleteJobCommand(job).Send();
                    completedJobs.Add(job);
                    if (completedJobs.Count == 3)
                    {
                        signal.Set();
                    }
                })
                .MaxJobsActive(3)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromSeconds(123L))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .PollingTimeout(TimeSpan.FromSeconds(5L))
                .Open())
            {
                Assert.True(jobWorker.IsOpen());
                signal.WaitOne();
            }

            // then
            var actualActivateRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualActivateRequest);

            var completeRequests = TestService.Requests[typeof(CompleteJobRequest)];
            while (completeRequests.Count != 3)
            {
                // busy loop to await 3 requests
                completeRequests = TestService.Requests[typeof(CompleteJobRequest)];
            }

            Assert.GreaterOrEqual(completeRequests.Count, 3);
            Assert.GreaterOrEqual(completedJobs.Count, 3);
            AssertJob(completedJobs[0], 1);
            AssertJob(completedJobs[1], 2);
            AssertJob(completedJobs[2], 3);
        }

        [Test]
        public void ShouldSendRequestsWithDifferentAmounts()
        {
            // given
            var expectedRequest = new ActivateJobsRequest
            {
                Timeout = 123_000L,
                MaxJobsToActivate = 4,
                Type = "foo",
                Worker = "jobWorker",
                RequestTimeout = 5_000L
            };

            var expectedSecondRequest = new ActivateJobsRequest
            {
                Timeout = 123_000L,
                MaxJobsToActivate = 1,
                Type = "foo",
                Worker = "jobWorker",
                RequestTimeout = 5_000L
            };

            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => CreateExpectedResponse());

            // when
            var receivedJobs = new List<IJob>();
            using (var jobWorker = ZeebeClient.NewWorker()
                .JobType("foo")
                .Handler((jobClient, job) =>
                {
                    // block job handling
                    using (var signal = new EventWaitHandle(false, EventResetMode.AutoReset))
                    {
                        signal.WaitOne();
                    }
                })
                .MaxJobsActive(4)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromSeconds(123L))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .PollingTimeout(TimeSpan.FromMilliseconds(5_000L))
                .Open())
            {
                Assert.True(jobWorker.IsOpen());
                while (TestService.Requests[typeof(ActivateJobsRequest)].Count < 2)
                {
                }
            }

            // then
            var actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualRequest);

            var actualSecondRequest = TestService.Requests[typeof(ActivateJobsRequest)][1];
            Assert.AreEqual(expectedSecondRequest, actualSecondRequest);
        }

        [Test]
        public void ShouldSendRequestWithTimeSpanTimeoutAsMilliseconds()
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
                .MaxJobsActive(1)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromMilliseconds(10_000L))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .PollingTimeout(TimeSpan.FromMilliseconds(5_000L))
                .Open())
            {
                Assert.True(jobWorker.IsOpen());
                signal.WaitOne();
            }

            // then
            var actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualRequest);

            Assert.AreEqual(receivedJobs.Count, 3);

            AssertJob(receivedJobs[0], 1);
            AssertJob(receivedJobs[1], 2);
            AssertJob(receivedJobs[2], 3);
        }

        [Test]
        public void ShouldSendRequestWithFetchVariables()
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
                .MaxJobsActive(1)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromSeconds(10))
                .FetchVariables("foo", "bar", "test")
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .Open())
            {
                Assert.True(jobWorker.IsOpen());
                signal.WaitOne();
            }

            // then
            var actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualRequest);

            Assert.AreEqual(receivedJobs.Count, 3);

            AssertJob(receivedJobs[0], 1);
            AssertJob(receivedJobs[1], 2);
            AssertJob(receivedJobs[2], 3);
        }

        [Test]
        public void ShouldSendRequestWithFetchVariablesList()
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
                .MaxJobsActive(1)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromSeconds(10))
                .FetchVariables(variableNames)
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .Open())
            {
                Assert.True(jobWorker.IsOpen());
                signal.WaitOne();
            }

            // then
            var actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualRequest);

            Assert.AreEqual(receivedJobs.Count, 3);

            AssertJob(receivedJobs[0], 1);
            AssertJob(receivedJobs[1], 2);
            AssertJob(receivedJobs[2], 3);
        }

        [Test]
        public void ShouldSendFailCommandOnExceptionInJobHandler()
        {
            // given
            var expectedRequest = new ActivateJobsRequest
            {
                Timeout = 123_000L,
                MaxJobsToActivate = 1,
                Type = "foo",
                Worker = "jobWorker"
            };

            var expectedFailRequest = new FailJobRequest
            {
                JobKey = 1,
                ErrorMessage = "Job worker 'jobWorker' tried to handle job of type 'foo', but exception occured 'Fail'",
                Retries = 2
            };

            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => CreateExpectedResponse());

            // when
            using (var jobWorker = ZeebeClient.NewWorker()
                .JobType("foo")
                .Handler((jobClient, job) =>
                {
                    if (job.Key == 1)
                    {
                        throw new Exception("Fail");
                    }
                })
                .MaxJobsActive(1)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromSeconds(123L))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .Open())
            {
                Assert.True(jobWorker.IsOpen());
                while (TestService.Requests[typeof(ActivateJobsRequest)].Count < 1
                       || TestService.Requests[typeof(FailJobRequest)].Count < 1)
                {
                }
            }

            // then
            var actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualRequest);

            var actualFailRequest = TestService.Requests[typeof(FailJobRequest)][0];
            Assert.AreEqual(expectedFailRequest, actualFailRequest);
        }

        [Test]
        public void ShouldCompleteAfterSendFailCommandOnExceptionInJobHandler()
        {
            // given
            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => CreateExpectedResponse());

            // when
            using (var jobWorker = ZeebeClient.NewWorker()
                .JobType("foo")
                .Handler(async (jobClient, job) =>
                {
                    if (job.Key == 2)
                    {
                        throw new Exception("Fail");
                    }

                    await jobClient.NewCompleteJobCommand(job).Send();
                })
                .MaxJobsActive(3)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromSeconds(123L))
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .Open())
            {
                Assert.True(jobWorker.IsOpen());
                while (TestService.Requests[typeof(ActivateJobsRequest)].Count < 1
                       || TestService.Requests[typeof(FailJobRequest)].Count < 1
                       || TestService.Requests[typeof(CompleteJobRequest)].Count < 2)
                {
                }
            }

            // then
            var completeRequests = TestService.Requests[typeof(CompleteJobRequest)];

            Assert.AreEqual(1, ((CompleteJobRequest)completeRequests[0]).JobKey);
            Assert.AreEqual(3, ((CompleteJobRequest)completeRequests[1]).JobKey);
        }

        [Test]
        public void ShouldUseAutoCompleteWithWorker()
        {
            // given
            var expectedRequest = new ActivateJobsRequest
            {
                Timeout = 5_000L,
                MaxJobsToActivate = 3,
                Type = "foo",
                Worker = "jobWorker",
                RequestTimeout = 5_000L
            };
            TestService.AddRequestHandler(
                typeof(ActivateJobsRequest),
                request => CreateExpectedResponse());

            // when
            using (var jobWorker = ZeebeClient.NewWorker()
                .JobType("foo")
                .Handler((jobClient, job) =>
                {
                    Logger.Info("Handler has seen job '{0}'", job);
                })
                .AutoCompletion()
                .MaxJobsActive(3)
                .Name("jobWorker")
                .Timeout(TimeSpan.FromMilliseconds(5_000L))
                .PollInterval(TimeSpan.FromSeconds(5))
                .PollingTimeout(TimeSpan.FromMilliseconds(5_000L))
                .Open())
            {
                Assert.True(jobWorker.IsOpen());
                while (TestService.Requests[typeof(CompleteJobRequest)].Count < 3)
                {
                }
            }

            // then
            var actualRequest = TestService.Requests[typeof(ActivateJobsRequest)][0];
            Assert.AreEqual(expectedRequest, actualRequest);

            var completeJobRequests = TestService.Requests[typeof(CompleteJobRequest)].OfType<CompleteJobRequest>().Select(j => j.JobKey).ToList();
            Assert.AreEqual(3, completeJobRequests.Count);

            Assert.Contains(1, completeJobRequests);
            Assert.Contains(2, completeJobRequests);
            Assert.Contains(3, completeJobRequests);
        }

        public static ActivateJobsResponse CreateExpectedResponse()
        {
            return new ActivateJobsResponse
            {
                Jobs =
                {
                    new ActivatedJob
                    {
                        Key = 1,
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
                    },
                    new ActivatedJob
                    {
                        Key = 2,
                        Worker = "jobWorker",
                        Type = "foo",
                        Variables = "{\"foo\":2}",
                        CustomHeaders = "{\"customFoo\":\"2\"}",
                        Retries = 3,
                        Deadline = 123932,
                        BpmnProcessId = "process",
                        ElementId = "job2",
                        ElementInstanceKey = 23,
                        WorkflowInstanceKey = 29,
                        WorkflowDefinitionVersion = 3,
                        WorkflowKey = 21
                    },
                    new ActivatedJob
                    {
                        Key = 3,
                        Worker = "jobWorker",
                        Type = "foo",
                        Variables = "{\"foo\":3}",
                        CustomHeaders = "{\"customFoo\":\"3\"}",
                        Retries = 3,
                        Deadline = 123932,
                        BpmnProcessId = "process",
                        ElementId = "job3",
                        ElementInstanceKey = 23,
                        WorkflowInstanceKey = 29,
                        WorkflowDefinitionVersion = 3,
                        WorkflowKey = 21
                    },
                },
            };
        }

        public static void AssertJob(IJob job, int expectedKey)
        {
            Assert.AreEqual(expectedKey, job.Key);
            Assert.AreEqual(3, job.Retries);
            Assert.AreEqual("foo", job.Type);
            Assert.AreEqual("jobWorker", job.Worker);

            Assert.AreEqual("{\"foo\":" + expectedKey + "}", job.Variables);
            Assert.AreEqual("{\"customFoo\":\"" + expectedKey + "\"}", job.CustomHeaders);

            Assert.AreEqual("process", job.BpmnProcessId);
            Assert.AreEqual("job" + expectedKey, job.ElementId);
            Assert.AreEqual(23, job.ElementInstanceKey);
            Assert.AreEqual(29, job.WorkflowInstanceKey);
            Assert.AreEqual(3, job.WorkflowDefinitionVersion);
            Assert.AreEqual(21, job.WorkflowKey);
        }
    }
}