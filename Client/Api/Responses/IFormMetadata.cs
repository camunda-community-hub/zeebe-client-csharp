namespace Zeebe.Client.Api.Responses;

public interface IFormMetadata
{
    /// <returns>the form ID of the deployed form.</returns>
    string FormId { get; }

    /// <returns>the version of the deployed form.</returns>
    int Version { get; }

    /// <returns>the key of the deployed form.</returns>
    long FormKey { get; }

    /// <returns>the name of the deployment resource which contains the form.</returns>
    string ResourceName { get; }
}
