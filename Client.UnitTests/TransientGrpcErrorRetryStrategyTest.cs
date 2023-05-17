using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;
using Zeebe.Client.Impl.Misc;

namespace Zeebe.Client;

[TestFixture]
public class TransientGrpcErrorRetryStrategyTest
{
    [Test]
    public async Task ShouldRetryOnResourceExhaustedException()
    {
        // given
        var retries = 0;
        var strategy = new TransientGrpcErrorRetryStrategy(_ => TimeSpan.Zero);

        // when
        var result = await strategy.DoWithRetry(() =>
        {
            if (retries == 3)
            {
                return Task.FromResult(retries);
            }

            retries++;
            throw new RpcException(new Status(StatusCode.ResourceExhausted, "resourceExhausted"));
        });

        // then
        Assert.AreEqual(3, result);
    }

    [Test]
    public async Task ShouldRetryOnUnavailableException()
    {
        // given
        var retries = 0;
        var strategy = new TransientGrpcErrorRetryStrategy(_ => TimeSpan.Zero);

        // when
        var result = await strategy.DoWithRetry(() =>
        {
            if (retries == 3)
            {
                return Task.FromResult(retries);
            }

            retries++;
            throw new RpcException(new Status(StatusCode.Unavailable, "resourceExhausted"));
        });

        // then
        Assert.AreEqual(3, result);
    }

    [Test]
    public async Task ShouldIncrementRetriesOnWaitTimeProvider()
    {
        // given
        var retries = 0;
        var values = new List<int>();
        var strategy = new TransientGrpcErrorRetryStrategy(retry =>
        {
            values.Add(retry);
            return TimeSpan.Zero;
        });

        // when
        var result = await strategy.DoWithRetry(() =>
        {
            if (retries == 3)
            {
                return Task.FromResult(retries);
            }

            retries++;
            throw new RpcException(new Status(StatusCode.ResourceExhausted, "resourceExhausted"));
        });

        // then
        Assert.AreEqual(3, result);
        CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, values);
    }

    [Test]
    public void ShouldWaitProvidedTime()
    {
        // given
        var retries = 0;
        var countdownEvent = new CountdownEvent(2);
        var strategy = new TransientGrpcErrorRetryStrategy(_ => TimeSpan.FromSeconds(1));

        // when
        var _ = strategy.DoWithRetry(() =>
        {
            countdownEvent.Signal();
            if (retries == 3)
            {
                return Task.FromResult(retries);
            }

            retries++;
            throw new RpcException(new Status(StatusCode.ResourceExhausted, "resourceExhausted"));
        });
        countdownEvent.Wait(TimeSpan.FromMilliseconds(10));

        // then
        Assert.AreEqual(countdownEvent.CurrentCount, 1);
        Assert.AreEqual(retries, 1);
    }

    [Test]
    public void ShouldNotRetryOnOtherRpcException()
    {
        // given
        var retries = 0;
        var strategy = new TransientGrpcErrorRetryStrategy(_ => TimeSpan.Zero);

        // when
        var task = strategy.DoWithRetry(() =>
        {
            if (retries == 3)
            {
                return Task.FromResult(retries);
            }

            retries++;
            throw new RpcException(new Status(StatusCode.Unknown, "idk"));
        });

        // then
        var rpcException = Assert.Throws<RpcException>(() => task.GetAwaiter().GetResult());
        Assert.AreEqual(StatusCode.Unknown, rpcException!.Status.StatusCode);
    }

    [Test]
    public void ShouldNotRetryOnOtherException()
    {
        // given
        var retries = 0;
        var strategy = new TransientGrpcErrorRetryStrategy(_ => TimeSpan.Zero);

        // when
        var task = strategy.DoWithRetry(() =>
        {
            if (retries == 3)
            {
                return Task.FromResult(retries);
            }

            retries++;
            throw new ApplicationException("exception");
        });

        // then
        var rpcException = Assert.Throws<ApplicationException>(() => task.GetAwaiter().GetResult());
        Assert.AreEqual("exception", rpcException!.Message);
    }
}