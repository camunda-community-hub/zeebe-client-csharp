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
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Commands;

namespace Zeebe.Client.Impl.Worker
{
    public class JobWorker : IJobWorker
    {
        private readonly ConcurrentQueue<IJob> workItems = new ConcurrentQueue<IJob>();
        private readonly CancellationTokenSource source;
        private readonly ILogger<JobWorker> logger;
        private readonly JobWorkerBuilder jobWorkerBuilder;

        private volatile bool isRunning;

        internal JobWorker(JobWorkerBuilder builder)
        {
            this.jobWorkerBuilder = builder;
            this.source = new CancellationTokenSource();
            this.logger = builder.LoggerFactory?.CreateLogger<JobWorker>();
        }

        /// <inheritdoc/>
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
        internal void Open()
        {
            isRunning = true;
            var cancellationToken = source.Token;
            var jobWorkerSignal = new JobWorkerSignal();

            StartPollerThread(jobWorkerSignal, cancellationToken);
            StartHandlerThreads(jobWorkerSignal, cancellationToken);

            var command = jobWorkerBuilder.Command;
            logger?.LogDebug(
                "Job worker ({worker}) for job type {type} has been opened.",
                command.Request.Worker,
                command.Request.Type);
        }

        private void StartPollerThread(JobWorkerSignal jobWorkerSignal, CancellationToken cancellationToken)
        {
            var poller = new JobPoller(jobWorkerBuilder, workItems, jobWorkerSignal);
            Task.Run(
                    async () =>
                        await poller.Poll(cancellationToken)
                            .ContinueWith(
                                t => logger?.LogError(t.Exception, "Job polling failed."),
                                TaskContinuationOptions.OnlyOnFaulted), cancellationToken)
                .ContinueWith(
                    t => logger?.LogError(t.Exception, "Job polling failed."),
                    TaskContinuationOptions.OnlyOnFaulted);
        }

        private void StartHandlerThreads(JobWorkerSignal jobWorkerSignal, CancellationToken cancellationToken)
        {
            var threadCount = jobWorkerBuilder.ThreadCount;
            for (var i = 0; i < threadCount; i++)
            {
                logger?.LogDebug("Start handler {index} thread", i);

                var jobHandlerExecutor = new JobHandlerExecutor(jobWorkerBuilder, workItems, jobWorkerSignal);
                Task.Run(async () => await jobHandlerExecutor.HandleActivatedJobs(cancellationToken), cancellationToken)
                    .ContinueWith(
                        t => logger?.LogError(t.Exception, "Job handling failed."),
                        TaskContinuationOptions.OnlyOnFaulted);
            }
        }
    }
}