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

    private const int StreamBackpressureDelayMs = 100;
    private const int StreamNoDataDelayMs = 10;
    private const int StreamErrorRetryDelayMs = 500;

    private readonly ActivateJobsRequest activateJobsRequest;
    private readonly StreamActivatedJobsRequest streamActivateJobsRequest;
    private readonly bool autoCompletion;
    private readonly JobActivator jobActivator;
    private readonly AsyncJobHandlerWithCancellationToken jobHandler;
    private readonly JobWorkerBuilder jobWorkerBuilder;
    private readonly ILogger<JobWorker> logger;
    private readonly int maxJobsActive;
    private readonly TimeSpan pollInterval;

    private readonly CancellationTokenSource localCts;
    private CancellationTokenSource? linkedCts;
    private readonly double thresholdJobsActivation;

    private int currentJobsActive;
    private volatile bool isRunning;

    internal JobWorker(JobWorkerBuilder builder)
    {
        jobWorkerBuilder = builder;
        localCts = new CancellationTokenSource();
        logger = builder.LoggerFactory?.CreateLogger<JobWorker>();
        jobHandler = jobWorkerBuilder.Handler();
        autoCompletion = builder.AutoCompletionEnabled();
        pollInterval = jobWorkerBuilder.PollInterval();
        activateJobsRequest = jobWorkerBuilder.Request;
        streamActivateJobsRequest = jobWorkerBuilder.StreamRequest;
        jobActivator = jobWorkerBuilder.Activator;
        maxJobsActive = jobWorkerBuilder.Request.MaxJobsToActivate;
        thresholdJobsActivation = maxJobsActive * 0.6;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        localCts.Cancel();
        // delay disposing, since poll and handler take some time to close
        _ = Task.Delay(TimeSpan.FromMilliseconds(pollInterval.TotalMilliseconds * 2))
        .ContinueWith(t =>
        {
            logger?.LogError("Dispose source");
            localCts.Dispose();
            linkedCts?.Dispose();
            linkedCts = null;
        });
        isRunning = false;
    }

    /// <inheritdoc />
    public bool IsOpen()
    {
        return isRunning;
    }

    /// <inheritdoc />
    public bool IsClosed()
    {
        return !isRunning;
    }

    /// <summary>
    ///     Opens the configured JobWorker to activate jobs in the given poll interval
    ///     and handle with the given handler.
    /// </summary>
    /// <param name="stoppingToken">The host cancellation token.</param>
    internal void Open(CancellationToken stoppingToken)
    {
        isRunning = true;
        this.linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, localCts.Token);
        var cancellationToken = linkedCts.Token;
        var bufferOptions = CreateBufferOptions(cancellationToken);
        var executionOptions = CreateExecutionOptions(cancellationToken);

        var input = new BufferBlock<IJob>(bufferOptions);
        var transformer = new TransformBlock<IJob, IJob>(
            async activatedJob => await HandleActivatedJob(activatedJob, cancellationToken),
            executionOptions);
        var output = new ActionBlock<IJob>(activatedJob => { _ = Interlocked.Decrement(ref currentJobsActive); },
            executionOptions);

        _ = input.LinkTo(transformer);
        _ = transformer.LinkTo(output);

        if (jobWorkerBuilder.GrpcStreamEnabled)
        {
            _ = Task.Run(async () => await StreamJobs(input, cancellationToken),
                cancellationToken).ContinueWith(
                t => logger?.LogError(t.Exception, "Job stream failed."),
                TaskContinuationOptions.OnlyOnFaulted);
        }

        // Start polling
        _ = Task.Run(async () => await PollJobs(input, cancellationToken),
        cancellationToken).ContinueWith(
        t => logger?.LogError(t.Exception, "Job polling failed."),
        TaskContinuationOptions.OnlyOnFaulted);

        logger?.LogDebug(
            "Job worker ({worker}) for job type {type} has been opened.",
            activateJobsRequest.Worker,
            activateJobsRequest.Type);
    }

    private async Task StreamJobs(ITargetBlock<IJob> input, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                logger?.LogDebug(
                    "Job worker stream ({worker}) for job type {type} is starting.",
                    activateJobsRequest.Worker,
                    activateJobsRequest.Type);

                var stream = jobActivator.SendStreamActivateRequest(streamActivateJobsRequest);

                logger?.LogDebug(
                    "Job worker stream ({worker}) for job type {type} has been opened.",
                    activateJobsRequest.Worker,
                    activateJobsRequest.Type);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var currentJobs = Thread.VolatileRead(ref currentJobsActive);

                    if (currentJobs >= thresholdJobsActivation)
                    {
                        await Task.Delay(StreamBackpressureDelayMs, cancellationToken);
                        continue;
                    }

                    if (!await stream.ResponseStream.MoveNext(cancellationToken))
                    {
                        logger?.LogDebug("Job stream MoveNext returned false; retrying shortly");
                        await Task.Delay(StreamNoDataDelayMs, cancellationToken);
                        continue;
                    }

                    var grpcActivatedJob = stream.ResponseStream.Current;
                    var activatedJob = new Zeebe.Client.Impl.Responses.ActivatedJob(grpcActivatedJob);
                    var response = new Zeebe.Client.Impl.Responses.ActivateJobsResponses();
                    response.Jobs.Add(activatedJob);

                    logger?.LogDebug(
                        "Job stream: new task received for worker {worker} with key {key}",
                        grpcActivatedJob.Worker,
                        grpcActivatedJob.Key);

                    await HandleActivationResponse(input, response, null, cancellationToken);
                }
            }
            catch (RpcException rpcException)
            {
                LogRpcException(rpcException);
                await Task.Delay(StreamErrorRetryDelayMs, cancellationToken);
            }
        }
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
        while (!cancellationToken.IsCancellationRequested)
        {
            var currentJobs = Thread.VolatileRead(ref currentJobsActive);
            if (currentJobs < thresholdJobsActivation)
            {
                var jobCount = maxJobsActive - currentJobs;
                activateJobsRequest.MaxJobsToActivate = jobCount;

                try
                {
                    await jobActivator.SendActivateRequest(activateJobsRequest,
                        async jobsResponse => await HandleActivationResponse(input, jobsResponse, jobCount, cancellationToken),
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

    private async Task HandleActivationResponse(
        ITargetBlock<IJob> input,
        IActivateJobsResponse response,
        int? jobCount,
        CancellationToken cancellationToken)
    {
        if (jobCount.HasValue)
        {
            logger?.LogDebug(
                "Job worker ({worker}) activated {activatedCount} of {requestCount} successfully.",
                activateJobsRequest.Worker,
                response.Jobs.Count,
                jobCount.Value);
        }
        else
        {
            logger?.LogDebug(
                "Job worker ({worker}) activated {activatedCount} from stream successfully.",
                activateJobsRequest.Worker,
                response.Jobs.Count);
        }

        foreach (var job in response.Jobs)
        {
            _ = await input.SendAsync(job, cancellationToken);
            _ = Interlocked.Increment(ref currentJobsActive);
        }
    }

    private async Task<IJob> HandleActivatedJob(IJob activatedJob, CancellationToken cancellationToken)
    {
        var jobClient = JobClientWrapper.Wrap(jobWorkerBuilder.JobClient);

        try
        {
            await jobHandler(jobClient, activatedJob, cancellationToken);
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
        var logLevel = rpcException.StatusCode switch
        {
            StatusCode.DeadlineExceeded or StatusCode.Cancelled or StatusCode.ResourceExhausted => LogLevel.Trace,
            _ => LogLevel.Error
        };

        logger?.Log(logLevel, rpcException, "Unexpected RpcException on polling new jobs.");
    }

    private async Task TryToAutoCompleteJob(JobClientWrapper jobClient, IJob activatedJob,
        CancellationToken cancellationToken)
    {
        if (!jobClient.ClientWasUsed && autoCompletion)
        {
            logger?.LogDebug(
                "Job worker ({worker}) will auto complete job with key '{key}'",
                activateJobsRequest.Worker,
                activatedJob.Key);
            _ = await jobClient.NewCompleteJobCommand(activatedJob)
          .Send(cancellationToken);
        }
    }

    private Task FailActivatedJob(JobClientWrapper jobClient, IJob activatedJob, CancellationToken cancellationToken,
        Exception exception)
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
                        logger?.LogWarning(task.Exception, "Problem on failing job occured.");
                    }
                }, cancellationToken);
    }
}
