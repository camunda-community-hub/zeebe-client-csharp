namespace Zeebe.Client.Api.Responses;

/// <summary>
/// Response for an set variables request.
/// </summary>
public interface ISetVariablesResponse
{
    /// <returns> The unique key of the command.</returns>
    long Key { get; }
}