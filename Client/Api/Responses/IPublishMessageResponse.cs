namespace Zeebe.Client.Api.Responses;

/// <summary>
///     Response for publishing a message.
/// </summary>
public interface IPublishMessageResponse
{
    /// <summary>
    ///     The unique ID of the message that was published.
    /// </summary>
    long Key { get; }

    /// <summary>
    ///     The tenant ID of the message.
    /// </summary>
    string TenantId { get; }
}