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
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Clients;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Subscription;
using Zeebe.Client.Impl.Commands;

namespace Zeebe.Client.Impl.Subscription
{
    public class JobWorker : IJobWorker
    {
        private const string JobFailMessage = "Job worker '{0}' tried to handle job of type '{1}', but exception occured '{2}'";

        private readonly int maxJobsActive;
        private readonly ConcurrentQueue<IJob> workItems = new ConcurrentQueue<IJob>();

        private readonly ActivateJobsRequest activeRequest;
        private readonly JobActivator activator;
        private readonly JobHandler jobHandler;
        private readonly IJobClient jobClient;

        private readonly Gateway.GatewayClient client;
        private readonly TimeSpan pollInterval;
        private readonly CancellationTokenSource source;

        private volatile bool isRunning;

        internal JobWorker(Gateway.GatewayClient client, ActivateJobsRequest request, TimeSpan pollInterval,
            IJobClient jobClient, JobHandler jobHandler)
        {
            this.source = new CancellationTokenSource();
            this.client = client;
            this.activator = new JobActivator(client);
            this.activeRequest = request;
            this.maxJobsActive = request.MaxJobsToActivate;
            this.pollInterval = pollInterval;
            this.jobClient = jobClient;
            this.jobHandler = jobHandler;
        }

        internal void Open()
        {
            isRunning = true;
            var cancellationToken = source.Token;

            var taskFactory = new TaskFactory();

            taskFactory.StartNew(async () =>
                await Poll(cancellationToken)
                    .ContinueWith(t => Console.WriteLine(t.Exception.ToString()),
                        TaskContinuationOptions.OnlyOnFaulted), cancellationToken
            ).ContinueWith(
                    t => Console.WriteLine(t.Exception.ToString()),
                    TaskContinuationOptions.OnlyOnFaulted);

            taskFactory.StartNew(() => HandleActivatedJobs(cancellationToken), cancellationToken)
                .ContinueWith(t => Console.WriteLine(t.Exception.ToString()),
                    TaskContinuationOptions.OnlyOnFaulted);
        }

        private void HandleActivatedJobs(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!workItems.IsEmpty)
                {
                    bool success = workItems.TryDequeue(out IJob activatedJob);

                    if (success)
                    {
                        try
                        {
                            jobHandler(jobClient, activatedJob);
                        }
                        catch (Exception exception)
                        {
                            FailActivatedJob(activatedJob, exception);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(pollInterval);
                }
            }
        }

        private void FailActivatedJob(IJob activatedJob, Exception exception)
        {
            string errorMessage = String.Format(JobFailMessage,
                activatedJob.Worker,
                activatedJob.Type,
                exception.Message);

            jobClient.NewFailCommand(activatedJob.Key)
                .Retries(activatedJob.Retries - 1)
                .ErrorMessage(errorMessage)
                .Send();
            Console.WriteLine(errorMessage);
        }

        private async Task Poll(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (workItems.Count < maxJobsActive)
                {
                    await PollJobs(cancellationToken);
                }
                Thread.Sleep(pollInterval);
            }
        }

        private async Task PollJobs(CancellationToken cancellationToken)
        {
            var jobCount = maxJobsActive - workItems.Count;
            activeRequest.MaxJobsToActivate = jobCount;

            var response = await activator.SendActivateRequest(activeRequest, cancellationToken);

            foreach (var job in response.Jobs)
            {
                workItems.Enqueue(job);
            }
        }

        public void Dispose()
        {
            source.Cancel();
            isRunning = false;
        }

        public bool IsOpen()
        {
            return isRunning;
        }

        public bool IsClosed()
        {
            return !isRunning;
        }
    }
}