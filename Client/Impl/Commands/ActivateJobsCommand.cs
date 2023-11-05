using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using static GatewayProtocol.Gateway;

namespace Zeebe.Client.Impl.Commands
{
    internal class ActivateJobsCommand : IActivateJobsCommandStep1, IActivateJobsCommandStep2, IActivateJobsCommandStep3
    {
        private readonly JobActivator activator;
        public ActivateJobsRequest Request { get; }
        private readonly IAsyncRetryStrategy asyncRetryStrategy;

        public ActivateJobsCommand(GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy)
        {
            this.asyncRetryStrategy = asyncRetryStrategy;
            activator = new JobActivator(client);
            Request = new ActivateJobsRequest();
        }

        public IActivateJobsCommandStep2 JobType(string jobType)
        {
            Request.Type = jobType;
            return this;
        }

        public IActivateJobsCommandStep1 AddTenantIds(IList<string> tenantIds)
        {
            Request.TenantIds.AddRange(tenantIds);

            return this;
        }

        public IActivateJobsCommandStep1 AddTenantIds(params string[] tenantIds)
        {
            Request.TenantIds.AddRange(tenantIds);

            return this;
         }

        public IActivateJobsCommandStep3 MaxJobsToActivate(int maxJobsToActivate)
        {
            Request.MaxJobsToActivate = maxJobsToActivate;
            return this;
        }

        public IActivateJobsCommandStep3 FetchVariables(IList<string> fetchVariables)
        {
            Request.FetchVariable.AddRange(fetchVariables);
            return this;
        }

        public IActivateJobsCommandStep3 FetchVariables(params string[] fetchVariables)
        {
            Request.FetchVariable.AddRange(fetchVariables);
            return this;
        }

        public IActivateJobsCommandStep3 Timeout(TimeSpan timeout)
        {
            Request.Timeout = (long)timeout.TotalMilliseconds;
            return this;
        }

        public IActivateJobsCommandStep3 PollingTimeout(TimeSpan pollingTimeout)
        {
            Request.RequestTimeout = (long)pollingTimeout.TotalMilliseconds;
            return this;
        }

        public IActivateJobsCommandStep3 WorkerName(string workerName)
        {
            Request.Worker = workerName;
            return this;
        }

        public async Task<IActivateJobsResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
        {
            var activateJobsResponses = new Responses.ActivateJobsResponses();
            await activator.SendActivateRequest(Request, response => Task.Run(() => activateJobsResponses.Add(response), token), timeout?.FromUtcNow(), token);
            return activateJobsResponses;
        }

        public async Task<IActivateJobsResponse> Send(CancellationToken cancellationToken)
        {
            return await Send(token: cancellationToken);
        }

        public async Task<IActivateJobsResponse> SendWithRetry(TimeSpan? timespan, CancellationToken cancellationToken = default)
        {
            return await asyncRetryStrategy.DoWithRetry(() => Send(timespan, cancellationToken));
        }
    }
}
