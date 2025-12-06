using System;

namespace Zeebe.Client.Api.Worker;

/// <summary>
/// Supplies the delay for the next retry after a failed activate request.
/// Value is in milliseconds; return value may be zero to retry immediately.
/// </summary>
public interface IBackoffSupplier
{
    /// <summary>
    /// Returns the delay before the next retry.
    /// </summary>
    /// <param name="currentRetryDelay">The previously used delay in milliseconds.</param>
    /// <returns>The new retry delay in milliseconds.</returns>
    long SupplyRetryDelay(long currentRetryDelay);
}
