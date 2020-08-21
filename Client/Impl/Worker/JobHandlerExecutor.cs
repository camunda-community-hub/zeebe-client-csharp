using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Commands;

namespace Zeebe.Client.Impl.Worker
{
    internal class JobHandlerExecutor
    {
        private const string JobFailMessage =
            "Job worker '{0}' tried to handle job of type '{1}', but exception occured '{2}'";

        private readonly ConcurrentQueue<IJob> workItems;
        private readonly JobClientWrapper jobClient;
        private readonly ILogger<JobWorker> logger;
        private readonly JobWorkerSignal jobWorkerSignal;
        private readonly TimeSpan pollInterval;
        private readonly AsyncJobHandler jobHandler;
        private readonly bool autoCompletion;
        private readonly ActivateJobsCommand activateJobsCommand;

        public JobHandlerExecutor(JobWorkerBuilder builder,
            ConcurrentQueue<IJob> workItems,
            JobWorkerSignal jobWorkerSignal)
        {
            this.jobClient = JobClientWrapper.Wrap(builder.JobClient);
            this.workItems = workItems;
            this.jobWorkerSignal = jobWorkerSignal;
            this.activateJobsCommand = builder.Command;
            this.pollInterval = builder.PollInterval();
            this.jobHandler = builder.Handler();
            this.autoCompletion = builder.AutoCompletionEnabled();
            this.logger = builder.LoggerFactory?.CreateLogger<JobWorker>();
        }

        public async Task HandleActivatedJobs(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!workItems.IsEmpty)
                {
                    bool success = workItems.TryDequeue(out IJob activatedJob);

                    if (success)
                    {
                        await HandleActivatedJob(cancellationToken, activatedJob);
                    }

                    jobWorkerSignal.SignalJobHandled();
                }
                else
                {
                    jobWorkerSignal.SignalJobHandled();
                    jobWorkerSignal.AwaitNewJobPolled(pollInterval);
                }
            }
        }

        private async Task HandleActivatedJob(CancellationToken cancellationToken, IJob activatedJob)
        {
            try
            {
                await jobHandler(jobClient, activatedJob);
                await TryToAutoCompleteJob(activatedJob);
            }
            catch (Exception exception)
            {
                await FailActivatedJob(activatedJob, cancellationToken, exception);
            }
            finally
            {
                jobClient.Reset();
            }
        }

        private async Task TryToAutoCompleteJob(IJob activatedJob)
        {
            if (!jobClient.ClientWasUsed && autoCompletion)
            {
                logger?.LogDebug(
                    "Job worker ({worker}) will auto complete job with key '{key}'",
                    activateJobsCommand.Request.Worker,
                    activatedJob.Key);
                await jobClient.NewCompleteJobCommand(activatedJob)
                    .Send();
            }
        }

        private Task FailActivatedJob(IJob activatedJob, CancellationToken cancellationToken, Exception exception)
        {
            var errorMessage = string.Format(
                JobFailMessage,
                activatedJob.Worker,
                activatedJob.Type,
                exception.Message);
            logger?.LogError(exception, errorMessage);

            return jobClient.NewFailCommand(activatedJob.Key)
                .Retries(activatedJob.Retries - 1)
                .ErrorMessage(errorMessage)
                .Send()
                .ContinueWith(
                    task =>
                    {
                        if (task.IsFaulted)
                        {
                            logger?.LogError("Problem on failing job occured.", task.Exception);
                        }
                    }, cancellationToken);
        }
    }
}