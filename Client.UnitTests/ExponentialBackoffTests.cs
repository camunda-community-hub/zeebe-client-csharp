using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Worker;

namespace Zeebe.Client;

[TestFixture]
public sealed class ExponentialBackoffTests
{
    [Test]
    public void ShouldReturnDelayWithinBounds_WhenNoJitter()
    {
        // given
        var maxDelay = TimeSpan.FromMilliseconds(1_000);
        var minDelay = TimeSpan.FromMilliseconds(50);
        IBackoffSupplier supplier = new ExponentialBackoffBuilderImpl()
            .MaxDelay(maxDelay)
            .MinDelay(minDelay)
            .BackoffFactor(1.6)
            .JitterFactor(0)
            .Build();

        var delays = new List<long>();
        long current = supplier.SupplyRetryDelay(0);

        // when
        for (var i = 0; i < 100; i++)
        {
            delays.Add(current);
            current = supplier.SupplyRetryDelay(current);
        }

        // then - with zero jitter, sequence should monotonically increase until it caps at max
        Assert.That(delays, Is.Not.Empty);
        Assert.That(delays.First(), Is.EqualTo((long)minDelay.TotalMilliseconds));
        Assert.That(delays.Last(), Is.EqualTo((long)maxDelay.TotalMilliseconds));

        long previous = -1;
        foreach (var delay in delays)
        {
            if (delay != (long)maxDelay.TotalMilliseconds)
            {
                Assert.That(delay, Is.GreaterThan(previous));
            }
            previous = delay;
        }
    }

    [Test]
    public void ShouldBeRandomizedWithJitter()
    {
        // given
        var maxDelay = TimeSpan.FromMilliseconds(1_000);
        var minDelay = TimeSpan.FromMilliseconds(50);
        const double jitterFactor = 0.2;

        IBackoffSupplier supplier = new ExponentialBackoffBuilderImpl()
            .MaxDelay(maxDelay)
            .MinDelay(minDelay)
            .BackoffFactor(1.5)
            .JitterFactor(jitterFactor)
            .Build();

        var lowerMaxBound = (long)Math.Round(maxDelay.TotalMilliseconds + (maxDelay.TotalMilliseconds * -jitterFactor));
        var upperMaxBound = (long)Math.Round(maxDelay.TotalMilliseconds + (maxDelay.TotalMilliseconds * jitterFactor));

        // when - always compute from max to test jitter around the cap
        var delays = new List<long>();
        for (var i = 0; i < 10; i++)
        {
            var value = supplier.SupplyRetryDelay((long)maxDelay.TotalMilliseconds);
            delays.Add(value);
        }

        // then
        Assert.That(delays, Is.Not.Empty);
        foreach (var delay in delays)
        {
            Assert.That(delay, Is.InRange(lowerMaxBound, upperMaxBound));
        }
        Assert.That(delays.Distinct().Count(), Is.GreaterThan(1));
    }
}

