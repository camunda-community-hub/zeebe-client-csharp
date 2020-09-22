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
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Commands;

namespace Zeebe.Client.Impl.Worker
{
    public class JobWorker : IJobWorker
    {
        private const string JobFailMessage =
            "Job worker '{0}' tried to handle job of type '{1}', but exception occured '{2}'";

        private readonly CancellationTokenSource source;
        private readonly ILogger<JobWorker> logger;
        private readonly JobWorkerBuilder jobWorkerBuilder;
        private readonly ActivateJobsCommand activateJobsCommand;
        private readonly int maxJobsActive;
        private readonly AsyncJobHandler jobHandler;
        private readonly bool autoCompletion;
        private readonly TimeSpan pollInterval;

        private volatile int currentJobsActive;
        private volatile bool isRunning;

        internal JobWorker(JobWorkerBuilder builder)
        {
            this.jobWorkerBuilder = builder;
            this.source = new CancellationTokenSource();
            this.logger = builder.LoggerFactory?.CreateLogger<JobWorker>();
            this.jobHandler = jobWorkerBuilder.Handler();
            this.autoCompletion = builder.AutoCompletionEnabled();
            this.pollInterval = jobWorkerBuilder.PollInterval();
            this.activateJobsCommand = jobWorkerBuilder.Command;
            this.maxJobsActive = jobWorkerBuilder.Command.Request.MaxJobsToActivate;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            source.Cancel();
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
        internal void Open()
        {
            isRunning = true;
            var cancellationToken = source.Token;
            var bufferOptions = CreateBufferOptions(cancellationToken);
            var executionOptions = CreateExecutionOptions(cancellationToken);

            var input = new BufferBlock<IJob>(bufferOptions);
            var transformer = new TransformBlock<IJob, IJob>(async activatedJob => await HandleActivatedJob(activatedJob, cancellationToken),
                executionOptions);
            var output = new ActionBlock<IJob>(activatedJob => { currentJobsActive--; },
                executionOptions);

            input.LinkTo(transformer);
            transformer.LinkTo(output);

            // Start polling
            Task.Run(async () => await PollJobs(input, cancellationToken),
                cancellationToken).ContinueWith(
                t => logger?.LogError(t.Exception, "Job polling failed."),
                TaskContinuationOptions.OnlyOnFaulted);

            logger?.LogDebug(
                "Job worker ({worker}) for job type {type} has been opened.",
                activateJobsCommand.Request.Worker,
                activateJobsCommand.Request.Type);
        }

        private ExecutionDataflowBlockOptions CreateExecutionOptions(CancellationToken cancellationToken)
        {
            return new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = jobWorkerBuilder.ThreadCount,
                CancellationToken = cancellationToken,
                EnsureOrdered = false
            };
        }

        private static DataflowBlockOptions CreateBufferOptions(CancellationToken cancellationToken)
        {
            return new DataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                EnsureOrdered = false
            };
        }

        private async Task PollJobs(ITargetBlock<IJob> input, CancellationToken cancellationToken)
        {
            while (!source.IsCancellationRequested)
            {
                if (currentJobsActive >= maxJobsActive)
                {
                    await Task.Delay(pollInterval, cancellationToken);
                    continue;
                }

                var jobCount = maxJobsActive - currentJobsActive;
                activateJobsCommand.MaxJobsToActivate(jobCount);

                try
                {
                    var response = await activateJobsCommand.Send(null, cancellationToken);
                    await HandleActivationResponse(input, response, jobCount);
                }
                catch (RpcException rpcException)
                {
                    LogRpcException(rpcException);
                }
            }
        }

        private async Task HandleActivationResponse(ITargetBlock<IJob> input, IActivateJobsResponse response, int jobCount)
        {
            logger?.LogDebug(
                "Job worker ({worker}) activated {activatedCount} of {requestCount} successfully.",
                activateJobsCommand.Request.Worker,
                response.Jobs.Count,
                jobCount);

            foreach (var job in response.Jobs)
            {
                await input.SendAsync(job);
                currentJobsActive++;
            }
        }

        private async Task<IJob> HandleActivatedJob(IJob activatedJob, CancellationToken cancellationToken)
        {
            var jobClient = JobClientWrapper.Wrap(jobWorkerBuilder.JobClient);

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

            return activatedJob;
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

        private async Task TryToAutoCompleteJob(JobClientWrapper jobClient, IJob activatedJob)
        {
            if (!jobClient.ClientWasUsed && autoCompletion)
            {
                logger?.LogDebug(
                    "Job worker ({worker}) will auto complete job with key '{key}'",
                    activateJobsCommand.Request.Worker,
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
    }
}