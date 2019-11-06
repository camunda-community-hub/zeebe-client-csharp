using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GatewayProtocol;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using static GatewayProtocol.Gateway;

namespace Zeebe.Client.Impl.Commands
{
    internal class ActivateJobsCommand : IActivateJobsCommandStep1, IActivateJobsCommandStep2, IActivateJobsCommandStep3
    {
        private readonly ActivateJobsRequest request;
        private readonly JobActivator activator;

        public ActivateJobsCommand(GatewayClient client)
        {
            activator = new JobActivator(client);
            request = new ActivateJobsRequest();
        }

        public IActivateJobsCommandStep2 JobType(string jobType)
        {
            request.Type = jobType;
            return this;
        }

        public IActivateJobsCommandStep3 MaxJobsToActivate(int maxJobsToActivate)
        {
            request.MaxJobsToActivate = maxJobsToActivate;
            return this;
        }

        public IActivateJobsCommandStep3 FetchVariables(IList<string> fetchVariables)
        {
            request.FetchVariable.AddRange(fetchVariables);
            return this;
        }

        public IActivateJobsCommandStep3 FetchVariables(params string[] fetchVariables)
        {
            request.FetchVariable.AddRange(fetchVariables);
            return this;
        }

        public IActivateJobsCommandStep3 Timeout(TimeSpan timeout)
        {
            request.Timeout = (long)timeout.TotalMilliseconds;
            return this;
        }

        public IActivateJobsCommandStep3 PollingTimeout(TimeSpan pollingTimeout)
        {
            request.RequestTimeout = (long)pollingTimeout.TotalMilliseconds;
            return this;
        }

        public IActivateJobsCommandStep3 WorkerName(string workerName)
        {
            request.Worker = workerName;
            return this;
        }

        public async Task<IActivateJobsResponse> Send(TimeSpan? timeout = null)
        {
            return await activator.SendActivateRequest(request, timeout?.FromUtcNow());
        }
    }
}
