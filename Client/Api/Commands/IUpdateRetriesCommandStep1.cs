using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands;

public interface IUpdateRetriesCommandStep1
{
    /// <summary>
    ///   Set the retries of this job.
    /// </summary>
    ///
    /// <para>
    /// If the given retries are greater than zero then this job will be picked up again by a job
    /// subscription and a related incident will be marked as resolved.
    /// </para>
    /// <param name="retries">retries the retries of this job.</param>
    /// <returns>
    /// the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.
    /// </returns>
    IUpdateRetriesCommandStep2 Retries(int retries);
}

public interface IUpdateRetriesCommandStep2 : IFinalCommandWithRetryStep<IUpdateRetriesResponse>
{
    // the place for new optional parameters
}