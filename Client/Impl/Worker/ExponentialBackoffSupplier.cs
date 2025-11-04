using System;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Impl.Worker;

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
        this.random = random ?? new Random();
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

