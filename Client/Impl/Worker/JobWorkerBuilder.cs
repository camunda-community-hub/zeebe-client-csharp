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

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GatewayProtocol;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Commands;

namespace Zeebe.Client.Impl.Worker;

public class JobWorkerBuilder(
    IZeebeClient zeebeClient,
    Gateway.GatewayClient gatewayClient,
    ILoggerFactory? loggerFactory = null)
    : IJobWorkerBuilderStep1, IJobWorkerBuilderStep2, IJobWorkerBuilderStep3
{
    internal TimeSpan PollingInterval { get; private set; }
    internal AsyncJobHandler? JobHandler { get; private set; }
    internal bool AutoCompletionEnabled { get; private set; }
    internal JobActivator Activator { get; } = new (gatewayClient);
    internal ActivateJobsRequest Request { get; } = new ();
    internal byte ThreadCount { get; set; } = 1;
    internal ILoggerFactory? LoggerFactory { get; } = loggerFactory;
    internal IJobClient JobClient { get; } = zeebeClient;

    public IJobWorkerBuilderStep2 JobType(string type)
    {
        Request.Type = type;
        return this;
    }

    public IJobWorkerBuilderStep3 Handler(JobHandler handler)
    {
        JobHandler = (c, j) => Task.Run(() => handler.Invoke(c, j));
        return this;
    }

    public IJobWorkerBuilderStep3 Handler(AsyncJobHandler handler)
    {
        JobHandler = handler;
        return this;
    }

    public IJobWorkerBuilderStep3 TenantIds(IList<string> tenantIds)
    {
        Request.TenantIds.AddRange(tenantIds);
        return this;
    }

    public IJobWorkerBuilderStep3 TenantIds(params string[] tenantIds)
    {
        return TenantIds(tenantIds.ToList());
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
        PollingInterval = pollInterval;
        return this;
    }

    public IJobWorkerBuilderStep3 PollingTimeout(TimeSpan pollingTimeout)
    {
        Request.RequestTimeout = (long) pollingTimeout.TotalMilliseconds;
        return this;
    }

    public IJobWorkerBuilderStep3 AutoCompletion()
    {
        AutoCompletionEnabled = true;
        return this;
    }

    public IJobWorkerBuilderStep3 HandlerThreads(byte threadCount)
    {
        if (threadCount <= 0)
        {
            var errorMsg = $"Expected an handler thread count larger then zero, but got {threadCount}.";
            throw new ArgumentOutOfRangeException(errorMsg);
        }

        ThreadCount = threadCount;
        return this;
    }

    public IJobWorker Open()
    {
        var worker = new JobWorker(this);

        worker.Open();

        return worker;
    }
}