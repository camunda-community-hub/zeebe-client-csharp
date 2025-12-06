using System;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Impl.Worker;

internal sealed class ExponentialBackoffBuilderImpl : IExponentialBackoffBuilder
{
    private TimeSpan maxDelay = TimeSpan.FromMilliseconds(5000);
    private TimeSpan minDelay = TimeSpan.FromMilliseconds(50);
    private double backoffFactor = 1.6;
    private double jitterFactor = 0.1;
    private Random random = new Random();

    public IExponentialBackoffBuilder MaxDelay(TimeSpan maxDelay)
    {
        this.maxDelay = maxDelay;
        return this;
    }

    public IExponentialBackoffBuilder MinDelay(TimeSpan minDelay)
    {
        this.minDelay = minDelay;
        return this;
    }

    public IExponentialBackoffBuilder BackoffFactor(double backoffFactor)
    {
        this.backoffFactor = backoffFactor;
        return this;
    }

    public IExponentialBackoffBuilder JitterFactor(double jitterFactor)
    {
        this.jitterFactor = jitterFactor;
        return this;
    }

    public IExponentialBackoffBuilder Random(Random random)
    {
        this.random = random ?? new Random();
        return this;
    }

    public IBackoffSupplier Build()
    {
        return new ExponentialBackoffSupplier(minDelay, maxDelay, backoffFactor, jitterFactor, random);
    }
}

