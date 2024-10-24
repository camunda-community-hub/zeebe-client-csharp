using System;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands;

public interface IUpdateJobTimeoutCommandStep1
{
    /// <summary>
    ///   Update the timeout for this job.
    /// </summary>
    ///
    /// <para>
    ///   If the job's timeout is set to zero, the job will be directly retried.
    /// </para>
    /// <param name="timeout">The duration of the new timeout as a TimeSpan, starting from the current moment.</param>
    /// <returns>
    ///   The builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.
    /// </returns>
    IUpdateJobTimeoutCommandStep2 Timeout(TimeSpan timeout);
}

public interface IUpdateJobTimeoutCommandStep2 : IFinalCommandWithRetryStep<IUpdateJobTimeoutResponse>
{
    // the place for new optional parameters
}