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
using System.Collections.Generic;
using System.Threading.Tasks;
using GatewayProtocol;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Commands;

namespace Zeebe.Client.Impl.Worker
{
    public class JobWorkerBuilder : IJobWorkerBuilderStep1, IJobWorkerBuilderStep2, IJobWorkerBuilderStep3
    {
        private TimeSpan pollInterval;
        private AsyncJobHandler asyncJobHandler;
        private bool autoCompletion;
        internal JobActivator Activator { get; }
        internal ActivateJobsRequest Request { get; }
        internal byte ThreadCount { get; set; }
        internal ILoggerFactory LoggerFactory { get; }
        internal IJobClient JobClient { get; }

        public JobWorkerBuilder(IZeebeClient zeebeClient,
            Gateway.GatewayClient gatewayClient,
            ILoggerFactory loggerFactory = null)
        {
            LoggerFactory = loggerFactory;
            Activator = new JobActivator(gatewayClient);
            Request = new ActivateJobsRequest();
            JobClient = zeebeClient;
            ThreadCount = 1;
        }

        public IJobWorkerBuilderStep2 JobType(string type)
        {
            Request.Type = type;
            return this;
        }

        public IJobWorkerBuilderStep3 Handler(JobHandler handler)
        {
            this.asyncJobHandler = (c, j) => Task.Run(() => handler.Invoke(c, j));
            return this;
        }

        public IJobWorkerBuilderStep3 Handler(AsyncJobHandler handler)
        {
            this.asyncJobHandler = handler;
            return this;
        }

        internal AsyncJobHandler Handler()
        {
            return asyncJobHandler;
        }

        public IJobWorkerBuilderStep3 Timeout(TimeSpan timeout)
        {
            Request.Timeout = (long) timeout.TotalMilliseconds;
            return this;
        }

        public IJobWorkerBuilderStep3 Name(string workerName)
        {
            Request.Worker = workerName;
            return this;
        }

        public IJobWorkerBuilderStep3 MaxJobsActive(int maxJobsActive)
        {
            Request.MaxJobsToActivate = maxJobsActive;
            return this;
        }

        public IJobWorkerBuilderStep3 FetchVariables(IList<string> fetchVariables)
        {
            Request.FetchVariable.AddRange(fetchVariables);
            return this;
        }

        public IJobWorkerBuilderStep3 FetchVariables(params string[] fetchVariables)
        {
            Request.FetchVariable.AddRange(fetchVariables);
            return this;
        }

        public IJobWorkerBuilderStep3 PollInterval(TimeSpan pollInterval)
        {
            this.pollInterval = pollInterval;
            return this;
        }

        internal TimeSpan PollInterval()
        {
            return pollInterval;
        }

        public IJobWorkerBuilderStep3 PollingTimeout(TimeSpan pollingTimeout)
        {
            Request.RequestTimeout = (long) pollingTimeout.TotalMilliseconds;
            return this;
        }

        public IJobWorkerBuilderStep3 AutoCompletion()
        {
            autoCompletion = true;
            return this;
        }

        public IJobWorkerBuilderStep3 HandlerThreads(byte threadCount)
        {
            if (threadCount <= 0)
            {
                var errorMsg = $"Expected an handler thread count larger then zero, but got {threadCount}.";
                throw new ArgumentOutOfRangeException(errorMsg);
            }

            this.ThreadCount = threadCount;
            return this;
        }

        internal bool AutoCompletionEnabled()
        {
            return autoCompletion;
        }

        public IJobWorker Open()
        {
            var worker = new JobWorker(this);

            worker.Open();

            return worker;
        }
    }
}
