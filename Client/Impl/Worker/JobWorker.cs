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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Commands;

namespace Zeebe.Client.Impl.Worker
{
    public class JobWorker : IJobWorker
    {
        private const string JobFailMessage =
            "Job worker '{0}' tried to handle job of type '{1}', but exception occured '{2}'";

        private readonly int maxJobsActive;
        private readonly ConcurrentQueue<IJob> workItems = new ConcurrentQueue<IJob>();

        private readonly ActivateJobsRequest activeRequest;
        private readonly JobActivator activator;
        private readonly AsyncJobHandler jobHandler;
//        private readonly JobClientWrapper jobClient;
        private readonly bool autoCompletion;
        private readonly TimeSpan pollInterval;
        private readonly CancellationTokenSource source;

        private readonly EventWaitHandle handleSignal = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly EventWaitHandle pollSignal = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly ILogger<JobWorker> logger;
        private volatile bool isRunning;
        private JobWorkerBuilder _jobWorkerBuilder;

        internal JobWorker(JobWorkerBuilder builder)
        {
            _jobWorkerBuilder = builder;
            source = new CancellationTokenSource();
            activator = new JobActivator(builder.Client);
            activeRequest = builder.Request;
            maxJobsActive = activeRequest.MaxJobsToActivate;
            pollInterval = builder.PollInterval();
            jobHandler = builder.Handler();
            autoCompletion = builder.AutoCompletionEnabled();
            logger = builder.LoggerFactory?.CreateLogger<JobWorker>();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            source.Cancel();
            // delay disposing, since poll and handler take some time to close
            Task.Delay(TimeSpan.FromMilliseconds(pollInterval.TotalMilliseconds * 2))
                .ContinueWith((t) =>
                {
                    logger?.LogError("Dispose source");
                    source.Dispose();
                });
            isRunning = false;
        }

        /// <inheritdoc/>
        public bool IsOpen()
        {
            return isRunning;
        }

        /// <inheritdoc/>
        public bool IsClosed()
        {
            return !isRunning;
        }

        /// <summary>
        /// Opens the configured JobWorker to activate jobs in the given poll interval
        /// and handle with the given handler.
        /// </summary>
        internal void Open()
        {
            isRunning = true;
            var cancellationToken = source.Token;

            Task.Run(
                    async () =>
                        await Poll(cancellationToken)
                            .ContinueWith(
                                t => logger?.LogError(t.Exception, "Job polling failed."),
                                TaskContinuationOptions.OnlyOnFaulted), cancellationToken)
                .ContinueWith(
                    t => logger?.LogError(t.Exception, "Job polling failed."),
                    TaskContinuationOptions.OnlyOnFaulted);

            var threadCount = 5;
            for (int i = 0; i < threadCount; i++)
            {
                logger?.LogError("Start handler {index}", i);
                Task.Run(async () => await HandleActivatedJobs(cancellationToken), cancellationToken)
                    .ContinueWith(
                        t => logger?.LogError(t.Exception, "Job handling failed."),
                        TaskContinuationOptions.OnlyOnFaulted);
            }

            logger?.LogDebug(
                "Job worker ({worker}) for job type {type} has been opened.",
                activeRequest.Worker,
                activeRequest.Type);
        }

        private async Task HandleActivatedJobs(CancellationToken cancellationToken)
        {
            var jobClient = JobClientWrapper.Wrap(_jobWorkerBuilder.JobClient);
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!workItems.IsEmpty)
                {
                    bool success = workItems.TryDequeue(out IJob activatedJob);

                    if (success)
                    {
                        await HandleActivatedJob(jobClient, cancellationToken, activatedJob);
                    }
                    else
                    {
                        pollSignal.Set();
                    }
                }
                else
                {
                    pollSignal.Set();
                    handleSignal.WaitOne(pollInterval);
                }
            }
        }

        private async Task HandleActivatedJob(JobClientWrapper jobClient, CancellationToken cancellationToken, IJob activatedJob)
        {
            try
            {
                await jobHandler(jobClient, activatedJob);
                await TryToAutoCompleteJob(jobClient, activatedJob);
            }
            catch (Exception exception)
            {
                await FailActivatedJob(jobClient, activatedJob, cancellationToken, exception);
            }
            finally
            {
                jobClient.Reset();
            }
        }

        private async Task TryToAutoCompleteJob(JobClientWrapper jobClient, IJob activatedJob)
        {
            if (!jobClient.ClientWasUsed && autoCompletion)
            {
                logger?.LogDebug(
                    "Job worker ({worker}) will auto complete job with key '{key}'",
                    activeRequest.Worker,
                    activatedJob.Key);
                await jobClient.NewCompleteJobCommand(activatedJob)
                    .Send();
            }
        }

        private Task FailActivatedJob(JobClientWrapper jobClient, IJob activatedJob, CancellationToken cancellationToken, Exception exception)
        {
            var errorMessage = string.Format(
                JobFailMessage,
                activatedJob.Worker,
                activatedJob.Type,
                exception.Message);
            logger?.LogError(exception, errorMessage);

            return jobClient.NewFailCommand(activatedJob.Key)
                .Retries(activatedJob.Retries - 1)
                .ErrorMessage(errorMessage)
                .Send()
                .ContinueWith(
                    task =>
                    {
                        if (task.IsFaulted)
                        {
                            logger?.LogError("Problem on failing job occured.", task.Exception);
                        }
                    }, cancellationToken);
        }

        private async Task Poll(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                while (workItems.Count < maxJobsActive)
                {
                    try
                    {
                        await PollJobs(cancellationToken);
                    }
                    catch (RpcException rpcException)
                    {
                        LogLevel logLevel;
                        switch (rpcException.StatusCode)
                        {
                            case StatusCode.DeadlineExceeded:
                            case StatusCode.Cancelled:
                                logLevel = LogLevel.Trace;
                                break;
                            default:
                                logLevel = LogLevel.Error;
                                break;
                        }

                        logger?.Log(logLevel, rpcException, "Unexpected RpcException on polling new jobs.");
                    }
                }

                pollSignal.WaitOne(pollInterval);
            }
        }

        private async Task PollJobs(CancellationToken cancellationToken)
        {
            var jobCount = maxJobsActive - workItems.Count;
            activeRequest.MaxJobsToActivate = jobCount;

            var response = await activator.SendActivateRequest(activeRequest, null, cancellationToken);

            logger?.LogDebug(
                "Job worker ({worker}) activated {activatedCount} of {requestCount} successfully.",
                activeRequest.Worker,
                response.Jobs.Count,
                jobCount);
            foreach (var job in response.Jobs)
            {
                workItems.Enqueue(job);
            }

            handleSignal.Set();
        }

        private class JobClientWrapper : IJobClient
        {
            public static JobClientWrapper Wrap(IJobClient client)
            {
                return new JobClientWrapper(client);
            }

            public bool ClientWasUsed { get; private set; }

            private IJobClient Client { get; }

            private JobClientWrapper(IJobClient client)
            {
                Client = client;
                ClientWasUsed = false;
            }

            public ICompleteJobCommandStep1 NewCompleteJobCommand(long jobKey)
            {
                ClientWasUsed = true;
                return Client.NewCompleteJobCommand(jobKey);
            }

            public IFailJobCommandStep1 NewFailCommand(long jobKey)
            {
                ClientWasUsed = true;
                return Client.NewFailCommand(jobKey);
            }

            public IThrowErrorCommandStep1 NewThrowErrorCommand(long jobKey)
            {
                ClientWasUsed = true;
                return Client.NewThrowErrorCommand(jobKey);
            }

            public void Reset()
            {
                ClientWasUsed = false;
            }

            public ICompleteJobCommandStep1 NewCompleteJobCommand(IJob activatedJob)
            {
                ClientWasUsed = true;
                return Client.NewCompleteJobCommand(activatedJob);
            }
        }
    }
}