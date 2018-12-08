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
using ActivatedJob = Zeebe.Client.Impl.Responses.ActivatedJob;

namespace Zeebe.Client.Impl.Subscription
{
    public class JobWorker : IJobWorker
    {
        private readonly ConcurrentQueue<IJob> workItems = new ConcurrentQueue<IJob>();

        private readonly ActivateJobsRequest activeRequest;
        private readonly Gateway.GatewayClient client;
        private readonly int pollInterval;
        private readonly CancellationTokenSource source;
        private readonly JobHandler jobHandler;
        private readonly IJobClient jobClient;

        private bool isRunning;

        internal JobWorker(Gateway.GatewayClient client, ActivateJobsRequest request, int pollInterval,
            IJobClient jobClient, JobHandler jobHandler)
        {
            source = new CancellationTokenSource();
            this.client = client;
            activeRequest = request;
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
                        TaskContinuationOptions.OnlyOnFaulted)
            ).ContinueWith(
                    t => Console.WriteLine(t.Exception.ToString()),
                    TaskContinuationOptions.OnlyOnFaulted);

            taskFactory.StartNew(() => HandleActivatedJobs())
                .ContinueWith(t => Console.WriteLine(t.Exception.ToString()),
                    TaskContinuationOptions.OnlyOnFaulted);
        }

        private void HandleActivatedJobs()
        {
            while (isRunning)
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
                            Console.WriteLine("Fail to handle job with values '{0}', job handler throws exception {1}", activatedJob, exception);
                            // TODO fail job
                        }
                    }
                }
                else
                {
                    Thread.Sleep(pollInterval);
                }
            }
        }

        private async Task Poll(CancellationToken cancellationToken)
        {
            while (isRunning)
            {
                await PollJobs(cancellationToken);
                Thread.Sleep(pollInterval);
            }
        }

        private async Task PollJobs(CancellationToken cancellationToken)
        {
            using (var stream = client.ActivateJobs(activeRequest))
            {
                var responseStream = stream.ResponseStream;
                while (await responseStream.MoveNext(cancellationToken))
                {
                    var response = responseStream.Current;
                    foreach (var job in response.Jobs)
                    {
                        workItems.Enqueue(new ActivatedJob(job));
                    }
                }
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