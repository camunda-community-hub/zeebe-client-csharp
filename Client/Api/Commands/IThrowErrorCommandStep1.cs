using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands
{
    public interface IThrowErrorCommandStep1
    {
        /// <summary>
        /// Set the errorCode for the error.
        /// </summary>
        /// <para>
        ///     If the errorCode can't be matched to an error catch event in the process, an incident will
        ///     be created.
        /// </para>
        /// <param name="errorCode">the errorCode that will be matched against an error catch event</param>
        /// <returns>the builder for this command</returns>
        IThrowErrorCommandStep2 ErrorCode(string errorCode);
    }

    public interface IThrowErrorCommandStep2 : IFinalCommandWithRetryStep<IThrowErrorResponse>
    {
        /// <summary>
        /// Provide an error message describing the reason for the non-technical error.
        /// </summary>
        /// If the error is not caught by an error catch event,
        /// this message will be a part of the raised incident.
        /// <param name="errorMessage">the error message</param>
        /// <returns>
        ///     The builder for this command.
        ///     Call <see cref="IFinalCommandStep{T}.Send"/> to complete the command and send it to the broker.
        /// </returns>
        IThrowErrorCommandStep2 ErrorMessage(string errorMessage);
    }
}
