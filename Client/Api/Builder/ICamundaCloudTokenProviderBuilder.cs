using Microsoft.Extensions.Logging;
using Zeebe.Client.Impl.Builder;

namespace Zeebe.Client.Api.Builder;

public interface ICamundaCloudTokenProviderBuilder
{
    /// <summary>
    /// Defines the logger factory which should be used by the token provider
    /// to log messages.
    /// *This is optional and no messages are logged if this method is not called.*.
    /// </summary>
    /// <param name="loggerFactory">the factory to create an ILogger.</param>
    /// <returns>the fluent ICamundaCloudTokenProviderBuilder.</returns>
    ICamundaCloudTokenProviderBuilder UseLoggerFactory(ILoggerFactory loggerFactory);

    /// <summary>
    /// Defines the authorization server, from which the access token should be requested.
    /// </summary>
    /// <param name="url">an url, which points to the authorization server.</param>
    /// <returns>the next step in building a CamundaCloudTokenProvider.</returns>
    ICamundaCloudTokenProviderBuilderStep2 UseAuthServer(string url);
}

public interface ICamundaCloudTokenProviderBuilderStep2
{
    /// <summary>
    /// Defines the client id, which should be used to create the access token.
    /// You need to create a client in the Camunda Cloud, after that you can find a newly
    /// generated client id there.
    /// </summary>
    /// <param name="clientId">the client id, which is supplied by the Camunda Cloud.</param>
    /// <returns>the next step in building a CamundaCloudTokenProvider.</returns>
    ICamundaCloudTokenProviderBuilderStep3 UseClientId(string clientId);
}

public interface ICamundaCloudTokenProviderBuilderStep3
{
    /// <summary>
    /// Defines the client secret, which should be used to create the access token.
    /// You need to create a client in the Camunda Cloud, after that you can find a newly
    /// generated client secret there.
    /// </summary>
    /// <param name="clientSecret">the client secret, which is supplied by the Camunda Cloud.</param>
    /// <returns>the next step in building a CamundaCloudTokenProvider.</returns>
    ICamundaCloudTokenProviderBuilderStep4 UseClientSecret(string clientSecret);
}

public interface ICamundaCloudTokenProviderBuilderStep4
{
    /// <summary>
    /// Defines the audience for which the token provider should create tokens.
    /// </summary>
    /// <param name="audience">the audience, which is normally a domain name.</param>
    /// <returns>the next step in building a CamundaCloudTokenProvider.</returns>
    ICamundaCloudTokenProviderBuilderFinalStep UseAudience(string audience);
}

public interface ICamundaCloudTokenProviderBuilderFinalStep
{
    /// <summary>
    /// Use given path to store credentials on disk.
    /// </summary>
    /// Per default credentials are stored in the home directory.
    /// <param name="path">The path where to store the credentials.</param>
    /// <returns>The final step in building a CamundaCloudTokenProvider.</returns>
    ICamundaCloudTokenProviderBuilderFinalStep UsePath(string path);

    /// <summary>
    /// Builds the CamundaCloudTokenProvider, which can be used by the ZeebeClient to
    /// communicate with the Camunda Cloud.
    /// </summary>
    /// <returns>the CamundaCloudTokenProvider.</returns>
    CamundaCloudTokenProvider Build();
}