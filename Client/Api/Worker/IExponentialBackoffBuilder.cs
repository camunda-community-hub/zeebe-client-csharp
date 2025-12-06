using System;

namespace Zeebe.Client.Api.Worker;

public interface IExponentialBackoffBuilder
{
    /// <summary>
    /// Sets the maximum retry delay. Default is 5000ms.
    /// </summary>
    IExponentialBackoffBuilder MaxDelay(TimeSpan maxDelay);

    /// <summary>
    /// Sets the minimum retry delay. Default is 50ms.
    /// </summary>
    IExponentialBackoffBuilder MinDelay(TimeSpan minDelay);

    /// <summary>
    /// Sets the multiplication factor (previous delay * factor). Default is 1.6.
    /// </summary>
    IExponentialBackoffBuilder BackoffFactor(double backoffFactor);

    /// <summary>
    /// Sets optional jitter factor (+/- factor of delay). Default is 0.1.
    /// </summary>
    IExponentialBackoffBuilder JitterFactor(double jitterFactor);

    /// <summary>
    /// Sets the random source used to compute jitter. Default is new Random().
    /// </summary>
    IExponentialBackoffBuilder Random(Random random);

    /// <summary>
    /// Builds the supplier with the provided configuration.
    /// </summary>
    IBackoffSupplier Build();
}
