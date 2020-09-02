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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Commands;
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client.Impl.Worker
{
    public class JobWorkerImprove : IJobWorker
    {
        private const string JobFailMessage =
            "Job worker '{0}' tried to handle job of type '{1}', but exception occured '{2}'";

        private readonly ConcurrentQueue<IJob> workItems = new ConcurrentQueue<IJob>();
        private readonly CancellationTokenSource source;
        private readonly ILogger<JobWorkerImprove> logger;
        private readonly JobWorkerBuilder jobWorkerBuilder;

        private volatile bool isRunning;
        private ActivateJobsCommand activateJobsCommand;
        private int threshold;
        private int maxJobsActive;

        private TimeSpan pollInterval;

        private JobClientWrapper jobClient;
        private AsyncJobHandler jobHandler;
        private bool autoCompletion;

//        private object jobWorkerSignal;
//        private JobWorkerSignal jobWorkerSignal;

        internal JobWorkerImprove(JobWorkerBuilder builder)
        {
            this.jobWorkerBuilder = builder;
            this.source = new CancellationTokenSource();
            this.logger = builder.LoggerFactory?.CreateLogger<JobWorkerImprove>();

            this.activateJobsCommand = builder.Command;
            this.threshold = (int) Math.Ceiling(activateJobsCommand.Request.MaxJobsToActivate * 0.6f);
            this.maxJobsActive = activateJobsCommand.Request.MaxJobsToActivate;
            this.workItems = workItems;
            this.pollInterval = builder.PollInterval();
//            this.logger = builder.LoggerFactory?.CreateLogger<JobWorker>();
//            this.jobWorkerS?/**/ignal = jobWorkerSignal;

            this.jobClient = JobClientWrapper.Wrap(builder.JobClient);
            this.workItems = workItems;
//            this.jobWorkerSignal = jobWorkerSignal;
            this.activateJobsCommand = builder.Command;
            this.pollInterval = builder.PollInterval();
            this.jobHandler = builder.Handler();
            this.autoCompletion = builder.AutoCompletionEnabled();
        }

//        /// <inheritdoc/>
        public void Dispose()
        {
            source.Cancel();
            var pollInterval = jobWorkerBuilder.PollInterval();
            // delay disposing, since poll and handler take some time to close
            Task.Delay(TimeSpan.FromMilliseconds(pollInterval.TotalMilliseconds * 2))
                .ContinueWith(t =>
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
        internal async Task Open()
        {
            isRunning = true;
            var cancellationToken = source.Token;

            var command = jobWorkerBuilder.Command;
            logger?.LogDebug(
                "Job worker ({worker}) for job type {type} has been opened.",
                command.Request.Worker,
                command.Request.Type);

            await Poll(cancellationToken);
        }

        private async Task Poll(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                int jobsCount;
                IActivateJobsResponse activateJobsResponse;
                do
                {
                    activateJobsResponse = await ActivateJobs(cancellationToken);

                    jobsCount = activateJobsResponse.Jobs.Count;
                    if (jobsCount == 0)
                    {
                        await Task.Delay(pollInterval, cancellationToken);
                    }
                }
                while (jobsCount == 0);

                var tasks = new List<Task>();
                foreach (var job in activateJobsResponse.Jobs)
                {
                    var handlerTask = Task.Run(async () => { await HandleActivatedJob(cancellationToken, job); }, cancellationToken);
                    tasks.Add(handlerTask);
                }

                await Task.WhenAll(tasks);
            }
        }

        private async Task HandleActivatedJob(CancellationToken cancellationToken, IJob activatedJob)
        {
            var clientWrapper = JobClientWrapper.Wrap(jobWorkerBuilder.JobClient);
            try
            {
                await jobHandler(clientWrapper, activatedJob);
                await TryToAutoCompleteJob(clientWrapper, activatedJob);
            }
            catch (Exception exception)
            {
                await FailActivatedJob(clientWrapper, activatedJob, cancellationToken, exception);
            }
            finally
            {
                clientWrapper.Reset();
            }
        }

        private async Task TryToAutoCompleteJob(JobClientWrapper clientWrapper, IJob activatedJob)
        {
            if (!clientWrapper.ClientWasUsed && autoCompletion)
            {
                logger?.LogDebug(
                    "Job worker ({worker}) will auto complete job with key '{key}'",
                    activateJobsCommand.Request.Worker,
                    activatedJob.Key);
                await clientWrapper.NewCompleteJobCommand(activatedJob)
                    .Send();
            }
        }

        private Task FailActivatedJob(JobClientWrapper clientWrapper, IJob activatedJob, CancellationToken cancellationToken, Exception exception)
        {
            var errorMessage = string.Format(
                JobFailMessage,
                activatedJob.Worker,
                activatedJob.Type,
                exception.Message);
            logger?.LogError(exception, errorMessage);

            return clientWrapper.NewFailCommand(activatedJob.Key)
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

        private async Task<IActivateJobsResponse> ActivateJobs(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            try
            {
                var jobCount = maxJobsActive - workItems.Count;
                activateJobsCommand.MaxJobsToActivate(jobCount);

                return await activateJobsCommand.Send(null, cancellationToken);
            }
            catch (RpcException rpcException)
            {
                LogRpcException(rpcException);
                return new ActivateJobsResponses();
            }
        }

        private void LogRpcException(RpcException rpcException)
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
}