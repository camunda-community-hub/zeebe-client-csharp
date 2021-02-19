using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using static GatewayProtocol.Gateway;

namespace Zeebe.Client.Impl.Commands
{
    internal class ActivateJobsCommand : IActivateJobsCommandStep1, IActivateJobsCommandStep2, IActivateJobsCommandStep3
    {
        private readonly JobActivator activator;
        public ActivateJobsRequest Request { get; }

        public ActivateJobsCommand(GatewayClient client)
        {
            activator = new JobActivator(client);
            Request = new ActivateJobsRequest();
        }

        public IActivateJobsCommandStep2 JobType(string jobType)
        {
            Request.Type = jobType;
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

        public async Task<IActivateJobsResponse> Send(TimeSpan? timeout = null)
        {
            return await Send(timeout, null);
        }

        public Task<IActivateJobsResponse> Send(CancellationToken token)
        {
            return Send(null, token);
        }

        public async Task<IActivateJobsResponse> Send(TimeSpan? timeout, CancellationToken? cancellationToken)
        {
            return await activator.SendActivateRequest(Request, timeout?.FromUtcNow(), cancellationToken);
        }

        public Task<IActivateJobsResponse> SendWithRetry(TimeSpan? timespan = null)
        {
            throw new NotImplementedException();
        }
    }
}
