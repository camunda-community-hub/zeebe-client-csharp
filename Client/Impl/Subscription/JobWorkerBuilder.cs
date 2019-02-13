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
using System.Collections.Generic;
using Zeebe.Client.Api.Clients;
using Zeebe.Client.Api.Subscription;

namespace Zeebe.Client.Impl.Subscription
{
    public class JobWorkerBuilder : IJobWorkerBuilderStep1, IJobWorkerBuilderStep2, IJobWorkerBuilderStep3
    {
        private readonly Gateway.GatewayClient gatewayClient;
        private readonly ActivateJobsRequest request = new ActivateJobsRequest();
        private readonly IJobClient jobClient;

        private JobHandler handler;
        private TimeSpan pollInterval;

        public JobWorkerBuilder(Gateway.GatewayClient client, IJobClient jobClient)
        {
            gatewayClient = client;
            this.jobClient = jobClient;
        }

        public IJobWorkerBuilderStep2 JobType(string type)
        {
            request.Type = type;
            return this;
        }

        public IJobWorkerBuilderStep3 Handler(JobHandler handler)
        {
            this.handler = handler;
            return this;
        }

        public IJobWorkerBuilderStep3 Timeout(long timeout)
        {
            request.Timeout = timeout;
            return this;
        }

        public IJobWorkerBuilderStep3 Timeout(TimeSpan timeout)
        {
            request.Timeout = (long)timeout.TotalMilliseconds;
            return this;
        }

        public IJobWorkerBuilderStep3 Name(string workerName)
        {
            request.Worker = workerName;
            return this;
        }

        public IJobWorkerBuilderStep3 Limit(int numberOfJobs)
        {
            request.Amount = numberOfJobs;
            return this;
        }

        public IJobWorkerBuilderStep3 FetchVariables(IList<string> fetchVariables)
        {
            request.FetchVariable.AddRange(fetchVariables);
            return this;
        }

        public IJobWorkerBuilderStep3 FetchVariables(params string[] fetchVariables)
        {
            request.FetchVariable.AddRange(fetchVariables);
            return this;
        }

        public IJobWorkerBuilderStep3 PollInterval(TimeSpan pollInterval)
        {
            this.pollInterval = pollInterval;
            return this;
        }

        public IJobWorker Open()
        {
            JobWorker worker = new JobWorker(gatewayClient, request, pollInterval, jobClient, handler);

            worker.Open();

            return worker;
        }
    }
}
