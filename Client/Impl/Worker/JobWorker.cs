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
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using GatewayProtocol;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Commands;

namespace Zeebe.Client.Impl.Worker;

public sealed class JobWorker : IJobWorker
{
    private const string JobFailMessage =
        "Job worker '{0}' tried to handle job of type '{1}', but exception occured '{2}'";

    private readonly CancellationTokenSource source;
    private readonly ILogger<JobWorker>? logger;
    private readonly JobWorkerBuilder jobWorkerBuilder;
    private readonly ActivateJobsRequest activateJobsRequest;
    private readonly JobActivator jobActivator;
    private readonly int maxJobsActive;
    private readonly AsyncJobHandler jobHandler;
    private readonly bool autoCompletion;
    private readonly TimeSpan pollInterval;
    private readonly double thresholdJobsActivation;

    private int currentJobsActive;
    private volatile bool isRunning;

    internal JobWorker(JobWorkerBuilder builder)
    {
        jobWorkerBuilder = builder;
        source = new CancellationTokenSource();
        logger = builder.LoggerFactory?.CreateLogger<JobWorker>();
        jobHandler = jobWorkerBuilder.JobHandler ?? throw new ArgumentNullException(nameof(jobWorkerBuilder.JobHandler));
        autoCompletion = builder.AutoCompletionEnabled;
        pollInterval = jobWorkerBuilder.PollingInterval;
        activateJobsRequest = jobWorkerBuilder.Request;
        jobActivator = jobWorkerBuilder.Activator;
        maxJobsActive = jobWorkerBuilder.Request.MaxJobsToActivate;
        thresholdJobsActivation = maxJobsActive * 0.6;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        source.Cancel();
        // delay disposing, since poll and handler take some time to close
        Task.Delay(TimeSpan.FromMilliseconds(pollInterval.TotalMilliseconds * 2))
            .ContinueWith(_ =>
            {
                logger?.LogError("Dispose source");
                source.Dispose();
            });
        isRunning = false;
    }

    /// <inheritdoc/>
    public bool IsOpen()
    {
        return isRunning;
    }

    /// <inheritdoc/>
    public bool IsClosed()
    {
        return !isRunning;
    }

    /// <summary>
    /// Opens the configured JobWorker to activate jobs in the given poll interval
    /// and handle with the given handler.
    /// </summary>
    internal void Open()
    {
        isRunning = true;
        var cancellationToken = source.Token;
        var bufferOptions = CreateBufferOptions(cancellationToken);
        var executionOptions = CreateExecutionOptions(cancellationToken);

        var input = new BufferBlock<IJob>(bufferOptions);
        var transformer = new TransformBlock<IJob, IJob>(async activatedJob => await HandleActivatedJob(activatedJob, cancellationToken),
            executionOptions);
        var output = new ActionBlock<IJob>(_ =>
            {
                Interlocked.Decrement(ref currentJobsActive);
            },
            executionOptions);

        input.LinkTo(transformer);
        transformer.LinkTo(output);

        // Start polling
        Task.Run(async () => await PollJobs(input, cancellationToken),
            cancellationToken).ContinueWith(
            t => logger?.LogError(t.Exception, "Job polling failed"),
            TaskContinuationOptions.OnlyOnFaulted);

        logger?.LogDebug(
            "Job worker ({Worker}) for job type {Type} has been opened",
            activateJobsRequest.Worker,
            activateJobsRequest.Type);
    }

    private ExecutionDataflowBlockOptions CreateExecutionOptions(CancellationToken cancellationToken)
    {
        return new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = jobWorkerBuilder.ThreadCount,
            CancellationToken = cancellationToken,
            EnsureOrdered = false
        };
    }

    private static DataflowBlockOptions CreateBufferOptions(CancellationToken cancellationToken)
    {
        return new DataflowBlockOptions
        {
            CancellationToken = cancellationToken,
            EnsureOrdered = false
        };
    }

    private async Task PollJobs(ITargetBlock<IJob> input, CancellationToken cancellationToken)
    {
        while (!source.IsCancellationRequested)
        {
            var currentJobs = Thread.VolatileRead(ref currentJobsActive);
            if (currentJobs < thresholdJobsActivation)
            {
                var jobCount = maxJobsActive - currentJobs;
                activateJobsRequest.MaxJobsToActivate = jobCount;

                try
                {
                    await jobActivator.SendActivateRequest(activateJobsRequest,
                        async jobsResponse => await HandleActivationResponse(input, jobsResponse, jobCount),
                        null,
                        cancellationToken);
                }
                catch (RpcException rpcException)
                {
                    LogRpcException(rpcException);
                    await Task.Delay(pollInterval, cancellationToken);
                }
            }
            else
            {
                await Task.Delay(pollInterval, cancellationToken);
            }
        }
    }

    private async Task HandleActivationResponse(ITargetBlock<IJob> input, IActivateJobsResponse response, int jobCount)
    {
        logger?.LogDebug(
            "Job worker ({Worker}) activated {ActivatedCount} of {RequestCount} successfully",
            activateJobsRequest.Worker,
            response.Jobs.Count,
            jobCount);

        foreach (var job in response.Jobs)
        {
            await input.SendAsync(job);
            Interlocked.Increment(ref currentJobsActive);
        }
    }

    private async Task<IJob> HandleActivatedJob(IJob activatedJob, CancellationToken cancellationToken)
    {
        var jobClient = JobClientWrapper.Wrap(jobWorkerBuilder.JobClient);

        try
        {
            await jobHandler(jobClient, activatedJob);
            await TryToAutoCompleteJob(jobClient, activatedJob, cancellationToken);
        }
        catch (Exception exception)
        {
            await FailActivatedJob(jobClient, activatedJob, cancellationToken, exception);
        }
        finally
        {
            jobClient.Reset();
        }

        return activatedJob;
    }

    private void LogRpcException(RpcException rpcException)
    {
        LogLevel logLevel;
        switch (rpcException.StatusCode)
        {
            case StatusCode.DeadlineExceeded:
            case StatusCode.Cancelled:
            case StatusCode.ResourceExhausted:
                logLevel = LogLevel.Trace;
                break;
            default:
                logLevel = LogLevel.Error;
                break;
        }

        logger?.Log(logLevel, rpcException, "Unexpected RpcException on polling new jobs");
    }

    private async Task TryToAutoCompleteJob(JobClientWrapper jobClient, IJob activatedJob,
        CancellationToken cancellationToken)
    {
        if (!jobClient.ClientWasUsed && autoCompletion)
        {
            logger?.LogDebug(
                "Job worker ({Worker}) will auto complete job with key '{Key}'",
                activateJobsRequest.Worker,
                activatedJob.Key);
            await jobClient.NewCompleteJobCommand(activatedJob)
                .Send(cancellationToken);
        }
    }

    private Task FailActivatedJob(JobClientWrapper jobClient, IJob activatedJob, CancellationToken cancellationToken, Exception exception)
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
            .Send(cancellationToken)
            .ContinueWith(
                task =>
                {
                    if (task.IsFaulted)
                    {
                        logger?.LogError(task.Exception, "Problem on failing job occured");
                    }
                }, cancellationToken);
    }
}