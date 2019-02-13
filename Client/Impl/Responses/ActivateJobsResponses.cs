using System.Collections.Generic;
using System.Linq;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class ActivateJobsResponses : IActivateJobsResponse
    {
        public IList<IJob> Jobs { get; set; }

        public ActivateJobsResponses()
        {
            Jobs = new List<IJob>();
        }

        public ActivateJobsResponses(GatewayProtocol.ActivateJobsResponse jobsResponse)
        {
            Jobs = jobsResponse.Jobs
                .Select(job => new ActivatedJob(job))
                .Cast<IJob>()
                .ToList();
        }
    }
}
