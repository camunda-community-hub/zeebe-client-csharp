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
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Commands;

namespace Zeebe.Client.Impl.Worker
{
    public class JobWorkerBuilder : IJobWorkerBuilderStep1, IJobWorkerBuilderStep2, IJobWorkerBuilderStep3
    {
        private TimeSpan pollInterval;
        private AsyncJobHandler handler;
        private bool autoCompletion;
        internal byte ThreadCount { get; set; }
        internal ILoggerFactory LoggerFactory { get; }
        internal ActivateJobsCommand Command { get; }
        internal IJobClient JobClient { get; }

        public JobWorkerBuilder(
            IZeebeClient zeebeClient,
            ILoggerFactory loggerFactory = null)
        {
            LoggerFactory = loggerFactory;
            Command = (ActivateJobsCommand) zeebeClient.NewActivateJobsCommand();
            JobClient = zeebeClient;
            ThreadCount = 1;

            zeebeClient.NewActivateJobsCommand();
        }

        public IJobWorkerBuilderStep2 JobType(string type)
        {
            Command.JobType(type);
            return this;
        }

        public IJobWorkerBuilderStep3 Handler(JobHandler handler)
        {
            this.handler = (c, j) => Task.Run(() => handler.Invoke(c, j));
            return this;
        }

        public IJobWorkerBuilderStep3 Handler(AsyncJobHandler handler)
        {
            this.handler = handler;
            return this;
        }

        internal AsyncJobHandler Handler()
        {
            return handler;
        }

        public IJobWorkerBuilderStep3 Timeout(TimeSpan timeout)
        {
            Command.Timeout(timeout);
            return this;
        }

        public IJobWorkerBuilderStep3 Name(string workerName)
        {
            Command.WorkerName(workerName);
            return this;
        }

        public IJobWorkerBuilderStep3 MaxJobsActive(int maxJobsActive)
        {
            Command.MaxJobsToActivate(maxJobsActive);
            return this;
        }

        public IJobWorkerBuilderStep3 FetchVariables(IList<string> fetchVariables)
        {
            Command.FetchVariables(fetchVariables);
            return this;
        }

        public IJobWorkerBuilderStep3 FetchVariables(params string[] fetchVariables)
        {
            Command.FetchVariables(fetchVariables);
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
            Command.PollingTimeout(pollingTimeout);
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
            var worker = new JobWorkerImprove(this);

            worker.Open();

            return worker;
        }
    }
}
