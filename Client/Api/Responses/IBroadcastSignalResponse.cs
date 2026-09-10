namespace Zeebe.Client.Api.Responses;

/// <summary>
///     Response for broadcasting a signal.
/// </summary>
public interface IBroadcastSignalResponse
{
    /// <summary>
    ///     The unique ID of the signal that was broadcasted.
    /// </summary>
    long Key { get; }

    /// <summary>
    ///     The tenant ID of the signal that was broadcasted.
    /// </summary>
    string TenantId { get; }
}