using Microsoft.Extensions.Logging;

namespace Zeebe.Client.Api.Builder;

/// <summary>
/// Simplifies the setup of an IZeebeClient which targets Camunda Cloud.
/// </summary>
public interface ICamundaCloudClientBuilder
{
    /// <summary>
    /// Defines the client id, which should be used to communicate with the Camunda Cloud cluster.
    ///
    /// You need to create a client in the Camunda Cloud, after that you can find a newly
    /// generated client id there.
    /// </summary>
    /// <param name="clientId">the client id, which is supplied by the Camunda Cloud.</param>
    /// <returns>the next step in building a ICamundaCloudClient.</returns>
    ICamundaCloudClientBuilderStep1 UseClientId(string clientId);

    /// <summary>
    /// Short cut operation.
    /// Reads from the environment all necessary information, to communicate with the camunda cloud cluster.
    ///
    /// Following environment variables are expected:
    /// <list type="bullet">
    /// <item>ZEEBE_ADDRESS</item>
    /// <item>ZEEBE_CLIENT_ID</item>
    /// <item>ZEEBE_CLIENT_SECRET</item>
    /// </list>
    ///
    /// Optional the authorization server url can be set via: ZEEBE_AUTHORIZATION_SERVER_URL.
    /// </summary>
    /// <returns>the final ICamundaCloudClientBuilder step.</returns>
    ICamundaCloudClientBuilderFinalStep FromEnv();
}

public interface ICamundaCloudClientBuilderStep1
{
    /// <summary>
    /// Defines the client secret, which should be used to communicate with the Camunda Cloud cluster.
    ///
    /// You need to create a client in the Camunda Cloud, after that you can find a newly
    /// generated client secret there.
    /// </summary>
    /// <param name="clientSecret">the client secret, which is supplied by the Camunda Cloud.</param>
    /// <returns>the next step in building a ICamundaCloudClient.</returns>
    ICamundaCloudClientBuilderStep2 UseClientSecret(string clientSecret);
}

public interface ICamundaCloudClientBuilderStep2
{
    /// <summary>
    /// The Camunda Cloud Cluster contact point, which should be used to communicate with.
    /// It is equal to the 'ZEEBE_ADDRESS' which you get from the Camunda Cloud Credentials.
    /// This will be used internally as OAuth audience as well.
    /// </summary>
    /// <param name="contactPoint">the audience, which is normally a domain name.</param>
    /// <returns>the next step in building a ICamundaCloudClient.</returns>
    ICamundaCloudClientBuilderFinalStep UseContactPoint(string contactPoint);
}

public interface ICamundaCloudClientBuilderFinalStep
{
    /// <summary>
    /// Defines the logger factory which should be used by the client.
    /// *This is optional and no messages are logged if this method is not called.*.
    /// </summary>
    /// <param name="loggerFactory">the factory to create an ILogger.</param>
    /// <returns>the fluent ICamundaCloudClientBuilderFinalStep.</returns>
    ICamundaCloudClientBuilderFinalStep UseLoggerFactory(ILoggerFactory loggerFactory);

    /// <summary>
    /// Defines the authorization server, from which the access token should be requested.
    /// </summary>
    /// <param name="url">an url, which points to the authorization server, if null it uses the default https://login.cloud.camunda.io/oauth/token.</param>
    /// <returns>the next step in building a ICamundaCloudClient.</returns>
    ICamundaCloudClientBuilderFinalStep UseAuthServer(string url);

    /// <summary>
    /// Sets the given path to store credentials on disk.
    /// </summary>
    /// <param name="path">the path where to store the credentials, if null it uses the default "~/.zeebe" .</param>
    /// <returns>the fluent ICamundaCloudClientBuilderFinalStep.</returns>
    ICamundaCloudClientBuilderFinalStep UsePersistedStoragePath(string path);

    /// <summary>
    /// The IZeebeClient, which is setup entirely to talk with the defined Camunda Cloud cluster.
    /// </summary>
    /// <returns>the IZeebeClient.</returns>
    IZeebeClient Build();
}