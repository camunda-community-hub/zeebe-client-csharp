using GatewayProtocol;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public IActivateJobsCommandStep3 Timeout(long timeout)
        {
            request.Timeout = timeout;
            return this;
        }

        public IActivateJobsCommandStep3 Timeout(TimeSpan timeout)
        {
            request.Timeout = (long)timeout.TotalMilliseconds;
            return this;
        }

        public IActivateJobsCommandStep3 WorkerName(string workerName)
        {
            request.Worker = workerName;
            return this;
        }

        public async Task<IActivateJobsResponse> Send()
        {
            return await activator.SendActivateRequest(request);
        }
    }
}
