using System.Collections.Generic;
using System.Linq;
using GatewayProtocol;
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
            Jobs = ConvertToList(jobsResponse);
        }

        public void Add(IActivateJobsResponse jobsResponse)
        {
            ((List<IJob>) Jobs).AddRange(jobsResponse.Jobs);
        }

        private static List<IJob> ConvertToList(ActivateJobsResponse jobsResponse)
        {
            return jobsResponse.Jobs
                .Select(job => new ActivatedJob(job))
                .Cast<IJob>()
                .ToList();
        }
    }
}
