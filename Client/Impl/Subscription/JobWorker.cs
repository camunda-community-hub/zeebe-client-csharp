//
//  Copyright 2018  camunda services gmbh
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

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Clients;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Subscription;

namespace Zeebe.Client.Impl.Subscription
{
    public class JobWorker : IJobWorker
    {
        private ConcurrentQueue<IJob> workItems = new ConcurrentQueue<IJob>();

        private readonly ActivateJobsRequest activeRequest;
        private readonly Gateway.GatewayClient client;
        private readonly int pollInterval;
        private readonly CancellationTokenSource source;
        private readonly JobHandler jobHandler;
        private readonly IJobClient jobClient;

        private bool isRunning;

        public JobWorker(Gateway.GatewayClient client, ActivateJobsRequest request, int pollInterval, IJobClient jobClient, JobHandler jobHandler)
        {
            this.source = new CancellationTokenSource();
            this.client = client;
            this.activeRequest = request;
            this.pollInterval = pollInterval;
            this.jobClient = jobClient;
            this.jobHandler = jobHandler;
        }


        internal void Open()
        {
            isRunning = true;
            CancellationToken cancellationToken = source.Token;

            var poller = new TaskFactory().StartNew(async () => await Poll(cancellationToken), cancellationToken);
            poller.Start();

            var handler = new TaskFactory().StartNew(() => HandleActivatedJobs(), cancellationToken);
            handler.Start();
        }

        private void HandleActivatedJobs()
        {
            while (isRunning && !workItems.IsEmpty)
            {
                bool success = workItems.TryDequeue(out IJob activatedJob);

                if (success)
                {
                    jobHandler.Invoke(jobClient, activatedJob);
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
            var stream = client.ActivateJobs(activeRequest).ResponseStream;
            while (await stream.MoveNext(cancellationToken))
            {
                ActivateJobsResponse response = stream.Current;
                foreach (ActivatedJob job in response.Jobs)
                {
                    workItems.Enqueue(new Responses.ActivatedJob(job));
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
