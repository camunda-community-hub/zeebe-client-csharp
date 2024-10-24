using System;
using System.Threading.Tasks;

namespace Zeebe.Client.Api.Misc;

public interface IAsyncRetryStrategy
{
    /// <summary>
    /// Runs the given asynchronous action asynchronously and retries it if it fails.
    /// When and how it is retried is depended of the implementation.
    /// </summary>
    /// <param name="action">the action which should be run and retried.</param>
    /// <typeparam name="TResult">the result type.</typeparam>
    /// <returns>the result of the action.</returns>
    Task<TResult> DoWithRetry<TResult>(Func<Task<TResult>> action);
}