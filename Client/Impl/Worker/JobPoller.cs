using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Commands;

namespace Zeebe.Client.Impl.Worker
{
    internal class JobPoller
    {
        private readonly ConcurrentQueue<IJob> workItems;
        private readonly int maxJobsActive;
        private readonly ILogger<JobWorker> logger;
        private readonly JobWorkerSignal jobWorkerSignal;
        private readonly TimeSpan pollInterval;
        private readonly ActivateJobsCommand activateJobsCommand;
        private int threshold;

        public JobPoller(JobWorkerBuilder builder,
            ConcurrentQueue<IJob> workItems,
            JobWorkerSignal jobWorkerSignal)
        {
            this.activateJobsCommand = builder.Command;
            this.threshold = (int) Math.Ceiling(activateJobsCommand.Request.MaxJobsToActivate * 0.6f);
            this.maxJobsActive = activateJobsCommand.Request.MaxJobsToActivate;
            this.workItems = workItems;
            this.pollInterval = builder.PollInterval();
            this.logger = builder.LoggerFactory?.CreateLogger<JobWorker>();
            this.jobWorkerSignal = jobWorkerSignal;
        }

        internal async Task Poll(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                while (workItems.Count < threshold)
                {
                    try
                    {
                        await PollJobs(cancellationToken);
                    }
                    catch (RpcException rpcException)
                    {
                        LogRpcException(rpcException);
                    }
                }

                jobWorkerSignal.AwaitJobHandling(pollInterval);
            }
        }

        private void LogRpcException(RpcException rpcException)
        {
            LogLevel logLevel;
            switch (rpcException.StatusCode)
            {
                case StatusCode.DeadlineExceeded:
                case StatusCode.Cancelled:
                    logLevel = LogLevel.Trace;
                    break;
                default:
                    logLevel = LogLevel.Error;
                    break;
            }

            logger?.Log(logLevel, rpcException, "Unexpected RpcException on polling new jobs.");
        }

        private async Task PollJobs(CancellationToken cancellationToken)
        {
            var jobCount = maxJobsActive - workItems.Count;
            activateJobsCommand.MaxJobsToActivate(jobCount);

            var response = await activateJobsCommand.Send(null, cancellationToken);

            logger?.LogDebug(
                "Job worker ({worker}) activated {activatedCount} of {requestCount} successfully.",
                activateJobsCommand.Request.Worker,
                response.Jobs.Count,
                jobCount);
            foreach (var job in response.Jobs)
            {
                workItems.Enqueue(job);
            }

            jobWorkerSignal.SignalJobPolled();
        }
    }
}