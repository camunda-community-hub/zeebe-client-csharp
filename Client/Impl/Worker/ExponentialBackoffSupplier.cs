using System;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Impl.Worker;

/// <summary>
/// An implementation of <see cref="IBackoffSupplier"/> which uses the **Exponential Backoff with Jitter** strategy
/// for calculating retry delays.
/// <para>
/// The implementation uses a simple formula: it multiplies the previous delay with a backoff multiplier
/// and adds some jitter to avoid multiple clients polling at the same time.
/// </para>
/// </summary>
/// <remarks>
/// The core logic is copied from the Zeebe Java client's
/// <c>io.camunda.zeebe.client.impl.worker.ExponentialBackoff</c> implementation
/// (source: <a href="https://github.com/camunda/camunda/blob/5764a0a3e6c3d3253c5c9608bf4478f8e2281af7/clients/java-deprecated/src/main/java/io/camunda/zeebe/client/impl/worker/ExponentialBackoff.java#L31">GitHub</a>).
/// <para>
/// The next delay is calculated by clamping the multiplied delay between <c>minDelay</c> and <c>maxDelay</c>,
/// and then adding a random jitter:
/// </para>
/// <c>max(min(maxDelay, currentDelay * backoffFactor), minDelay) + jitter</c>
/// <para>The final result is ensured to be non-negative and rounded.</para>
/// </remarks>
internal sealed class ExponentialBackoffSupplier : IBackoffSupplier
{
    private readonly double maxDelayMs;
    private readonly double minDelayMs;
    private readonly double backoffFactor;
    private readonly double jitterFactor;
    private readonly Random random;

    internal ExponentialBackoffSupplier(
        TimeSpan minDelay,
        TimeSpan maxDelay,
        double backoffFactor,
        double jitterFactor,
        Random random)
    {
        maxDelayMs = maxDelay.TotalMilliseconds;
        minDelayMs = minDelay.TotalMilliseconds;
        this.backoffFactor = backoffFactor;
        this.jitterFactor = jitterFactor;
        this.random = random ?? Random.Shared;
    }

    public long SupplyRetryDelay(long currentRetryDelay)
    {
        var previous = (double)currentRetryDelay;
        var multiplied = previous * backoffFactor;
        var clamped = Math.Max(Math.Min(maxDelayMs, multiplied), minDelayMs);

        var range = clamped * jitterFactor;
        var jitter = (random.NextDouble() * (2 * range)) - range;

        var next = clamped + jitter;
        if (next < 0)
        {
            next = 0;
        }

        return (long)Math.Round(next);
    }
}

